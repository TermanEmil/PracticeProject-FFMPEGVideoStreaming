using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ProcessStreamer;
using VideoStreamer.Models.LogFileViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VideoStreamer.Controllers
{
    
    public class LogFileController : Controller
    {
        private readonly FFMPEGConfig _ffmpegConfig;
        private readonly List<StreamConfig> _streamsConfig;

        public LogFileController(IConfiguration configuration)
        {
            _ffmpegConfig = configuration.GetSection("FFMPEGConfig")
                                         .Get<FFMPEGConfig>();
            _streamsConfig = new List<StreamConfig>();
            configuration.GetSection("StreamsConfig").Bind(_streamsConfig);
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
                content = PlaylistGenerator.GeneratePlaylist(
                    channel,
                    DateTime.Now,
                    _ffmpegConfig,
                    _streamsConfig,
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
