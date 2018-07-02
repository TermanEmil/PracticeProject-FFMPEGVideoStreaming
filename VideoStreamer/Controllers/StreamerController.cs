using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ProcessStreamer;

namespace VideoStreamer.Controllers
{
    [Route("api")]
	public class StreamerController : Controller
    {
		private readonly FFMPEGConfig _ffmpegConfig;
		private readonly List<StreamConfig> _streamsConfig;

		public StreamerController(IConfiguration configuration)
		{
			_ffmpegConfig = configuration.GetSection("FFMPEGConfig")
                                         .Get<FFMPEGConfig>();
			_streamsConfig = new List<StreamConfig>();
            configuration.GetSection("StreamsConfig").Bind(_streamsConfig);
		}

		[Route("LiveStream/{chanel}/index.m3u8")]
		public async Task<IActionResult> LiveStreamAsync(
			string chanel,
		    int listSize = 5)
		{
			var time = DateTime.Now;
			return await Task.Run(
				() => GetPlaylistActionResult(chanel, time, listSize));
		}

		[Route("TimeShift/{chanel}/{timeShiftMills}/index.m3u8")]
        public async Task<IActionResult> TimeShiftStreamAsync(
			string chanel,
			int timeShiftMills,
			int listSize = 5)
        {
			var timeNow = DateTime.Now;
			var time = timeNow;
			if (timeShiftMills > 0)
				time = time.AddMilliseconds(-timeShiftMills);
   
            return await Task.Run(
				() => GetPlaylistActionResult(chanel, time, listSize));
        }

		private IActionResult GetPlaylistActionResult(
			string chanel,
			DateTime time,
		    int hlsListSize)
		{
			var content = "";

			try
			{
				content = PlaylistGenerator.GeneratePlaylist(
					chanel,
					time,
					_ffmpegConfig,
					_streamsConfig,
					hlsListSize
				);
			}
			catch (Exception e)
			{
				return new JsonResult(e.Message);
			}
            
			var bytes = Encoding.UTF8.GetBytes(content);
			var result = new FileContentResult(bytes, "text/utf8")
			{
				FileDownloadName = "index.m3u8"
			};
                     
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
				_ffmpegConfig.ChunkStorageDir,
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
			
            Console.WriteLine("Requested TsFile {0} -> {1}", DateTime.Now, path);
			return new FileStreamResult(
				System.IO.File.OpenRead(path),
				"video/vnd.dlna.mpeg-tts");
		}
    }
}
