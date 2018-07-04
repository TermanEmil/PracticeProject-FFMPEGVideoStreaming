﻿using System;
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

namespace VideoStreamer.Controllers
{
    [Route("api")]
	public class StreamerController : Controller
    {
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly List<StreamConfig> _streamsCfg;

		public StreamerController(IConfiguration cfg)
		{
			ConfigLoader.Load(cfg, out _ffmpegCfg, out _streamsCfg);
		}

		[Route("Stream/{chanel}/index.m3u8")]
		public async Task<IActionResult> StreamAsync(
			string chanel,
			int listSize = 5,
		    int timeShiftMills = 0)
		{
			var timeNow = DateTime.Now;
            var time = timeNow;
            if (timeShiftMills > 0)
                time = time.AddMilliseconds(-timeShiftMills);

            return await Task.Run(
                () => GetPlaylistActionResult(chanel, time, listSize));
		}

		public static string GetRawPlaylist(
			ISession session,
			FFMPEGConfig ffmpegCfg,
			IEnumerable<StreamConfig> streamsCfgs,
			string channel,
            DateTime time,
            int hlsListSize,
			out Exception exception)
		{
			var content = "";

            var reqState = session.GetStreamRequestState();
			var hash = reqState?.GetHashCode();
            if (reqState == null)
            {
                reqState = new StreamRequestState
                {
                    Channel = channel,
                    ReferenceTime = DateTime.Now
                };

                // Require value to be updated.
                hash = 0;
            }

			var m3u8Generator = new PlaylistGenerator();
			try
			{
				content = m3u8Generator.GenerateM3U8Str(
					channel,
					time,
					ffmpegCfg,
					streamsCfgs,
					ref reqState,
					hlsListSize
				);
				exception = null;
			}
			catch (NoSuchChannelException e)
			{ exception = e; }
			catch (NoAvailableFilesException e)
			{ exception = e; }

			if (reqState.GetHashCode() != hash)
                session.SetStreamRequestState(reqState);

			return exception != null ? null : content;
		}

		private IActionResult GetPlaylistActionResult(
			string channel,
			DateTime time,
		    int hlsListSize)
		{
			var content = GetRawPlaylist(
				HttpContext.Session,
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

		[Route("{mode}/{chanel}/{year}/{month}/{day}/{hour}/{minute}/{fileName}")]
		public IActionResult GetChunkFile(
			string mode,
			string chanel,
			string year,
			string month,
			string day,
			string hour,
			string minute,
			string fileName)
		{
			var path = Path.Combine(
				_ffmpegCfg.ChunkStorageDir,
				chanel,
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
