using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.TokenBrokers;
using FFMPEGStreamingTools.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
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
		private readonly IDistributedCache _cache;
		private readonly ITokenBroker _tokenBroker;
		private readonly StreamerSessionCfg _sessionCfg;

		public StreamerController(
			IServiceProvider serviceProvider,
			IConfiguration cfg,
			IM3U8Generator m3u8Generator,
			StreamerContext dbContext,
			IDistributedCache cache,
			ITokenBroker tokenBroker)
		{
			FFMPEGConfigLoader.Load(cfg, out _ffmpegCfg, out _streamsCfg);
			_m3u8Generator = m3u8Generator;
			_dbContext = dbContext;
			_cache = cache;
			_tokenBroker = tokenBroker;

			_sessionCfg = cfg.GetSection("StreamingSessionsConfig")
			                 .Get<StreamerSessionCfg>();
			_sessionCfg.CheckForEnvironmentalues();
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
				() => StreamInternalAsync(
					channel,
					listSize,
					timeStr,
					timeShiftMills,
					displayContent)
			);
		}

		private async Task<IActionResult> StreamInternalAsync(
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
				{ return View("InvalidTimeFormat"); }
			}

			if (timeShiftMills != 0)
				requiredTime = requiredTime.AddMilliseconds(-timeShiftMills);

			StreamingSession session;         
			try
			{
				session = InitializeSession(
					channel,
					requiredTime,
					listSize,
					displayContent);
			}
            catch (NoAvailableFilesException e)
            { return Content(e.Message); }
            catch (NoSuchChannelException e)
            { return Content(e.Message); }

			var token = _tokenBroker.GenerateToken(
				channel + GetConnectionDetails(),
				_sessionCfg.TokenSALT);
			
			try
			{
				await _cache.SetObjAsync(
					token,
					session,
					GetSessionExpiration());
			}
			catch (Exception)
			{ return RedisConnectionException(); }

			return RedirectToAction("TokenizedStreamAsync", new { token });
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

			return new StreamingSession
            {
                Channel = channel,
                HlsListSize = listSize,
				ConnectionDetails = GetConnectionDetails(),
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
				() => TokenizedStreamInternalAsync(channel, token));
		}

		private async Task<IActionResult> TokenizedStreamInternalAsync(
			string channel,
			string token = null)
		{
			var tuple = await BasicTokenValidationsAsync(token, channel);
            var session = tuple.Item1;
            var invalidRequestActionResult = tuple.Item2;

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

				try
				{
					await _cache.SetObjAsync(
						token,
						session,
						GetSessionExpiration());
				}
				catch (Exception)
				{ return RedisConnectionException(); }

				var result = playlist.Bake($"token={token}");
				if (session.DisplayContent)
					return Content(result);
				else
					return GenerateDownloadableContent(result);
			}
			catch (NoAvailableFilesException e)
			{ return new JsonResult(e.Message); }
			catch (NoSuchChannelException e)
			{ return new JsonResult(e.Message); }
		}

		[Route("{mode}/{channel}/{year}/{month}/{day}/{hour}/{minute}/{fileName}")]
		public async Task<IActionResult> GetChunkFileAsync(
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
			var tuple = await BasicTokenValidationsAsync(token, channel);
			var session = tuple.Item1;
			var invalidRequestActionResult = tuple.Item2;

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

		private async
		Task<Tuple<StreamingSession, IActionResult>> BasicTokenValidationsAsync(
			string token,
			string channel)
		{         
			while (true)
			{
				if (token == null)
					break;

				StreamingSession session;            
				try
				{
					session = await _cache.GetAsync<StreamingSession>(token);
				}
				catch (Exception)
                {
					return new Tuple<StreamingSession, IActionResult>(
						null, RedisConnectionException());
                }

				if (session == null)
					break;

				if (channel != session.Channel)
					break;
    
				if (session.ConnectionDetails != GetConnectionDetails())
				{
					return new Tuple<StreamingSession, IActionResult>(
						null, Unauthorized());
				}

				return new Tuple<StreamingSession, IActionResult>(
					session, null);
			}

			return new Tuple<StreamingSession, IActionResult>(
				null, NotFound());
		}
        
		private string GetConnectionDetails()
		{
			var userAgent = HttpContext.Request.Headers[HeaderNames.UserAgent];
            var ip = HttpContext.Connection.RemoteIpAddress;

			return $"userAgent: '{userAgent}'; ip: {ip}";
		}
        
		private DistributedCacheEntryOptions GetSessionExpiration()
		{
			return new DistributedCacheEntryOptions()
				.SetAbsoluteExpiration(
					TimeSpan.FromSeconds(
						_sessionCfg.ExpirationTimeSeconds));
		}

		private IActionResult RedisConnectionException()
		{
			return Content("The database server is not responding.");
		}
        #endregion
	}
}
