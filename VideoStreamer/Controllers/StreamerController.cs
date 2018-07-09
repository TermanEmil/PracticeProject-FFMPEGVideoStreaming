using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VideoStreamer.DB;
using VideoStreamer.Models.Configs;
using VideoStreamer.Utils;

namespace VideoStreamer.Controllers
{
	[Route("api")]
	public class StreamerController : Controller
	{
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly List<StreamConfig> _streamsCfg;
		private readonly IM3U8Generator _m3u8Generator;
		private readonly StreamerContext _dbContext;
		private readonly StreamerSessionCfg _sessionCfg;

		public StreamerController(
			IServiceProvider serviceProvider,
			IConfiguration cfg,
			IM3U8Generator m3u8Generator,
			StreamerContext dbContext)
		{
			FFMPEGConfigLoader.Load(cfg, out _ffmpegCfg, out _streamsCfg);
			_m3u8Generator = m3u8Generator;
			_dbContext = dbContext;
			_sessionCfg = cfg.GetSection("StreamingSessionsConfig")
			                 .Get<StreamerSessionCfg>();

			StreamerSessionCleaner.TryCleanupExpiredSession(serviceProvider)
			                      .Wait();
		}
        
		[Route("Stream/{channel}/index.m3u8")]
		public async Task<IActionResult> StreamAsync(
			string channel,
			int listSize = 5,
			string timeStr = null,
			int timeShiftMills = 0,
			bool displayContent = false)
		{
			return await Task.Run(
				() => Stream(
					channel,
					listSize,
					timeStr,
					timeShiftMills,
					displayContent)
			);
		}

		private IActionResult Stream(
			string channel,
			int listSize = 5,
			string timeStr = null,
			int timeShiftMills = 0,
			bool displayContent = false)
		{
			DateTime requiredTime;

			if (timeStr == null)
				requiredTime = DateTime.Now;
			else
			{
				try
				{
					requiredTime =
						JsonConvert.DeserializeObject<DateTime>(timeStr);
				}
				catch (JsonReaderException)
				{
					return View("InvalidTimeFormat");
				}
			}

			if (timeShiftMills != 0)
				requiredTime = requiredTime.AddMilliseconds(-timeShiftMills);

			try
			{
				var session = InitializeSession(
					channel, requiredTime, listSize, displayContent);
				
				_dbContext.StreamingSessions.Add(session);
				_dbContext.SaveChanges();

				return RedirectToAction(
					"TokenizedStreamAsync",
					new { token = session.ID }
				);
			}
			catch (Exception e)
			{
				return new JsonResult(e.Message);
			}
		}

		private StreamingSession InitializeSession(
			string channel,
			DateTime requiredTime,
		    int listSize,
			bool displayContent)
		{
			var playlist = _m3u8Generator.GenerateM3U8(
                _ffmpegCfg,
                _streamsCfg,
                channel,
                requiredTime,
                listSize);

            var firstFile =
                new ChunkFile(playlist.files.First().filePath);

            var lastFileTime =
                TimeTools.SecondsToDateTime(firstFile.timeSeconds)
                         .Add(DateTimeOffset.Now.Offset);
                         
            var expTime =
                DateTime.Now.AddSeconds(
					_sessionCfg.ExpirationTimeSeconds);

			return new StreamingSession
            {
                Channel = channel,
                HlsListSize = listSize,
                ExpireTime = expTime,
                IP = HttpContext.Connection.RemoteIpAddress.ToString(),
                LastFileIndex = firstFile.index - 1,
                LastFileTimeSpan = lastFileTime,
                DisplayContent = displayContent
            };
		}

		[Route("TokenizedStream/{channel}/index.m3u8")]
		public async Task<IActionResult> TokenizedStreamAsync(
			string channel,
			string token = null)
		{
			return await Task.Run(
				() => TokenizedStream(channel, token));
		}

		private IActionResult TokenizedStream(
			string channel,
			string token = null)
		{
			var session = BasicTokenValidations(
				token, channel, out var invalidRequestActionResult);

			if (session == null)
				return invalidRequestActionResult;

			try
			{
				var playlist = _m3u8Generator.GenerateNextM3U8(
					_ffmpegCfg,
					_streamsCfg,
					channel,
					session.HlsListSize,
					session.LastFileIndex,
					session.LastFileTimeSpan);

				Console.WriteLine(
					"[StreamCtrl]:> m3u8: {0} TOKEN: {1}",
					string.Join(
						", ",
						playlist.files.Select(x => x.fileIndex)),
					session.LastFileIndex
				);

				var firstFile =
					new ChunkFile(playlist.files.First().filePath);

				session.LastFileIndex = firstFile.index;
				session.LastFileTimeSpan =
					TimeTools.SecondsToDateTime(firstFile.timeSeconds)
							 .Add(DateTimeOffset.Now.Offset);

				session.ExpireTime =
		            DateTime.Now
					       .AddSeconds(_sessionCfg.ExpirationTimeSeconds);

				_dbContext.Update(session);
				_dbContext.SaveChanges();

				var result = playlist.Bake($"token={token}");
				if (session.DisplayContent)
					return Content(result);
				else
					return GenerateDownloadableContent(result);
			}
			catch (Exception e)
			{
				return new JsonResult(e.Message);
			}
		}

		[Route("{mode}/{channel}/{year}/{month}/{day}/{hour}/{minute}/{fileName}")]
		public IActionResult GetChunkFile(
			string mode,
			string channel,
			string year,
			string month,
			string day,
			string hour,
			string minute,
			string fileName,
			string token = null)
		{
			var session = BasicTokenValidations(
                token, channel, out var invalidRequestActionResult);

            if (session == null)
                return invalidRequestActionResult;         

			var path = Path.Combine(
				_ffmpegCfg.ChunkStorageDir,
				channel,
				year,
				month,
				day,
				hour,
				minute,
				fileName
			);

			if (!System.IO.File.Exists(path))
				return NotFound();

			Console.WriteLine(
				"[StreamCtrl]:> Requested TsFile {0} -> {1}",
				DateTime.Now,
				path);

			return new FileStreamResult(
				System.IO.File.OpenRead(path),
				"video/vnd.dlna.mpeg-tts");
		}

		#region Helpers
		private IActionResult GenerateDownloadableContent(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var result = new FileContentResult(bytes, "text/utf8")
            {
                FileDownloadName = "index.m3u8"
            };

            return result;
        }

		private StreamingSession BasicTokenValidations(
			string token,
			string channel,
			out IActionResult actionResult)
		{
			while (true)
			{
				if (token == null)
					break;

				var session = _dbContext.StreamingSessions.Find(token);
				if (session == null)
					break;

                if (DateTime.Now > session.ExpireTime)
                {
                    _dbContext.StreamingSessions.Remove(session);
                    _dbContext.SaveChanges();
					break;
                }

				if (channel != session.Channel)
					break;
    
                if (session.IP !=
				    HttpContext.Connection.RemoteIpAddress.ToString())
				{
					actionResult = Unauthorized();
					return null;
				}

				actionResult = null;
				return session;
			}

			actionResult = NotFound();
			return null;
		}
        #endregion
	}
}
