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
	[Route("api/[controller]")]
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
		public async Task<IActionResult> LiveStreamAsync(string chanel)
		{
			return await Task.Run(() => LiveStream(chanel));
		}

		private IActionResult LiveStream(string chanel)
		{
			var content = "";

			try
			{
				content = PlaylistGenerator.GeneratePlaylist(
					chanel,
					DateTime.Now,
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
