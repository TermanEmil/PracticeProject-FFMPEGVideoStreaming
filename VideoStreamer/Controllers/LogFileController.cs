using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.StreamingSettings;
using VideoStreamer.Models.LogFileViewModels;
using FFMPEGStreamingTools.Utils;
using FFMPEGStreamingTools.M3u8Generators;
using Microsoft.AspNetCore.Http;
using VideoStreamer.Utils;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VideoStreamer.Controllers
{
    
    public class LogFileController : Controller
    {
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly List<StreamConfig> _streamsCfg;
		private readonly PlaylistGenerator _m3u8Generator;
		private readonly IConfiguration _cfg;

		public LogFileController(IConfiguration cfg)
        {
			_cfg = cfg;
			ConfigLoader.Load(cfg, out _ffmpegCfg, out _streamsCfg);
			_m3u8Generator = new PlaylistGenerator();
        }

        [Route("api/[controller]/{channel}")]
        public IActionResult Index(string channel, int millSecondRequest = 1000, int hlsListSize = 5)
        {
            return View(new LogFileModel
            {
                Channel = channel,
                HlsListSize = hlsListSize,
                Interval = millSecondRequest
            });
        }

        [HttpPost("api/[controller]/GetData")]
        public JsonResult GetLastPlayerFile(
            string channel = "",
            int millSecondRequest = 1000,
            int hlsListSize = 5,
		    int timeShiftMills = 0)
        {
			var content = StreamerController.GetRawPlaylist(
				HttpContext.Session,
				_ffmpegCfg, _streamsCfg,
				channel,
				DateTime.Now.AddMilliseconds(-timeShiftMills),
				hlsListSize,
				out var exception);

			if (exception != null)
				content = exception.Message;
            
            return  Json(new 
            {
                content = content.Replace("\n", "<br>")
            });
        }

    }
}
