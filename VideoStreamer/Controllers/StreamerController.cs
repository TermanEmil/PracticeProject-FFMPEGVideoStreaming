using System;
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

		public StreamerController(IConfiguration configuration)
		{
			_ffmpegConfig = configuration.GetSection("FFMPEGConfig")
                                            .Get<FFMPEGConfig>();
		}

		[Route("LiveStream")]
		[HttpGet("{chanel}")]
		public IActionResult LiveStream(string chanel)
		{
			var chanelRoot = string.Format(
				"./{0}/{1}",
				_ffmpegConfig.ChunkStorageDir,
				chanel.ToLower());

			if (!Directory.Exists(chanelRoot))
				return NotFound();

			int hlsLstSize = 5;
			var content = String.Join("\n", new[]
			{
				"#EXTM3U",
                "#EXT-X-VERSION:3",
                "#EXT-X-TARGETDURATION:6",
                "#EXT-X-MEDIA-SEQUENCE:0",
			});

			//var newestDir = string.Format(
			//"{0}/{1}/{2}",
			//DateTime.Ut.);
			var lastModifTime = Directory.GetLastWriteTime(chanelRoot);
			var newestDir =
				$"{lastModifTime.Year}/" +
				$"{lastModifTime.Month}/" +
				$"{lastModifTime.Day}/" +
				$"{lastModifTime.Hour}/" +
				$"{lastModifTime.Minute}";

            // get all possible files depending on the delta time, and get the latest n files or smth.

			for (int i = 0; i < hlsLstSize; i++)
			{
				
			}
            
			var bytes = Encoding.UTF8.GetBytes(content);
			var result = new FileContentResult(bytes, "text/utf8")
			{
				FileDownloadName = "index.m3u8"
			};

			return result;
		}

		public IActionResult GetChunkFile()
		{
			return null;
		}
    }
}
