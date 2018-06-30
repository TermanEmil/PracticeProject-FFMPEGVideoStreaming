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

		[Route("api/LiveStream/{chanel}/index.m3u8")]
		public async Task<IActionResult> LiveStreamAsync(string chanel)
		{
			var time = DateTime.Now;
			return await Task.Run(
				() => GetPlaylistActionResult(chanel, time));
		}

		[Route("api/TimeShift/{chanel}/index_now-{timeShiftMills}.m3u8")]
        public async Task<IActionResult> TimeShiftStreamAsync(
			string chanel,
			int timeShiftMills)
        {
			var timeNow = DateTime.Now;
			var time = timeNow;
			if (timeShiftMills > 0)
				time = time.AddMilliseconds(-timeShiftMills);
   
            return await Task.Run(() => GetPlaylistActionResult(chanel, time));
        }

		private IActionResult GetPlaylistActionResult(
			string chanel,
			DateTime time)
		{
			var content = "";

			try
			{
				content = PlaylistGenerator.GeneratePlaylist(
					chanel,
					time,
					_ffmpegConfig,
					_streamsConfig
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

		[Route("LiveStream/{chanel}/{year}/{month}/{day}/{hour}/{minute}/{fileName}")]
		public IActionResult GetChunkFile(
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
   
			var bytes = System.IO.File.ReadAllBytes(path);

			Debug.WriteLine("RquestTSFile: " + fileName);

			return new FileStreamResult(
				System.IO.File.OpenRead(path),
				"video/vnd.dlna.mpeg-tts");
		}
    }
}
