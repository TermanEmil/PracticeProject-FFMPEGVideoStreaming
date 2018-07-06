using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;
using FFMPEGStreamingTools.M3u8Generators;
using VideoStreamer.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using VideoStreamer.DB;

namespace VideoStreamer.Controllers
{
    [Route("api")]
	public class StreamerController : Controller
    {
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly List<StreamConfig> _streamsCfg;

		public StreamerController(
			IConfiguration cfg,
		    StreamerContext dbContext)
		{
			ConfigLoader.Load(cfg, out _ffmpegCfg, out _streamsCfg);
		}

		[Route("Stream/{channel}/index.m3u8")]
		public async Task<IActionResult> StreamAsync(
			string channel,
			int listSize = 5,
			string timeStr = null,
		    int timeShiftMills = 0,
			string token = null)
		{
			DateTime requiredTime;
            
			if (timeStr == null)
				requiredTime = DateTime.Now;
			else
			{
				try
				{
					requiredTime = ProcessFixedTime(timeStr, channel);
				}
				catch (JsonReaderException)
				{
					return new ContentResult
					{
						Content = "Invalid time format. " +
							"Example: 2018-07-04T16:52:00%2B03:00, " +
							"where '%2B' stands for '+'"
					};
				}            
			}

            if (timeShiftMills > 0)
                requiredTime = requiredTime.AddMilliseconds(-timeShiftMills);
			
            return await Task.Run(
                () => GetPlaylistActionResult(channel, requiredTime, listSize));
		}

		private DateTime ProcessFixedTime(
			string timeStr,
			string channel)
		{
			var time = JsonConvert.DeserializeObject<DateTime>(timeStr);
                     
            var reqState = HttpContext.Session.GetStreamRequestState();
            if (reqState == null ||
                reqState.Channel != channel ||
                reqState.ReferenceTime != time)
            {
                reqState = new StreamRequestState
                {
                    Channel = channel,
                    ReferenceTime = time,
                    TimeDifference = DateTime.Now - time
                };
                HttpContext.Session.SetStreamRequestState(reqState);            
            }

			var result = DateTime.Now.Add(-reqState.TimeDifference);
			return result;
		}

		public static string GetRawPlaylist(
			FFMPEGConfig ffmpegCfg,
			IEnumerable<StreamConfig> streamsCfgs,
			string channel,
            DateTime time,
            int hlsListSize,
			out Exception exception)
		{
			var content = "";

			var m3u8Generator = new PlaylistGenerator();
			try
			{
				content = m3u8Generator.GenerateM3U8Str(
					channel,
					time,
					ffmpegCfg,
					streamsCfgs,
					hlsListSize
				);
				exception = null;
			}
			catch (NoSuchChannelException e)
			{ exception = e; }
			catch (NoAvailableFilesException e)
			{ exception = e; }
            
			return exception != null ? null : content;
		}

		private IActionResult GetPlaylistActionResult(
			string channel,
			DateTime time,
		    int hlsListSize)
		{
			var content = GetRawPlaylist(
				_ffmpegCfg, _streamsCfg,          
				channel,
				time,
				hlsListSize,
				out var exception);

			if (exception != null)
				return new JsonResult(exception.Message);
   
			var bytes = Encoding.UTF8.GetBytes(content);
			var result = new FileContentResult(bytes, "text/utf8")
			{
				FileDownloadName = "index.m3u8"
			};
            
			Console.WriteLine("[StreamCtrl]:> Requested m3u8 {0}", DateTime.Now);

			return result;
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
    }
}
