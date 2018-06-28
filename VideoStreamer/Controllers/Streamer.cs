using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VideoStreamer.Controllers
{
	[Route("api/[controller]")]
	public class Streamer : Controller
    {
		[Route("LiveStream")]
		[HttpGet("{chanel}/index.m3u8")]
		public IActionResult LiveStream(string chanel)
		{
            

			var content = "";


			var bytes = Encoding.UTF8.GetBytes(content);
			var result = new FileContentResult(bytes, "text")
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
