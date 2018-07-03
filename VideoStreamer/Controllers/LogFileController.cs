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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VideoStreamer.Controllers
{
    
    public class LogFileController : Controller
    {
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly List<StreamConfig> _streamsCfg;
		private readonly PlaylistGenerator _m3u8Generator;

		public LogFileController(IConfiguration cfg)
        {
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
            string channel ="",
            int millSecondRequest = 1000,
            int hlsListSize = 5)
        {
            var content = "";

            try
            {
				content = _m3u8Generator.GenerateM3U8Str(
                    channel,
                    DateTime.Now,
					_ffmpegCfg,
					_streamsCfg,
                    hlsListSize
                );
            }
            catch (Exception e)
            {
                content = e.Message;
            }

            return  Json(new 
            {
                content = content.Replace("\n", "<br>")
            });
        }

    }
}
