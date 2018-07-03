using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ProcessStreamer;
using VideoStreamer.Models.LivePlayerViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VideoStreamer.Controllers
{
    [Route("api/[controller]/{channel}")]
    public class LivePlayerController : Controller
    {
        private readonly FFMPEGConfig _ffmpegConfig;
        private readonly List<StreamConfig> _streamsConfig;

        public LivePlayerController(IConfiguration configuration)
        {
            _ffmpegConfig = configuration.GetSection("FFMPEGConfig")
                                         .Get<FFMPEGConfig>();
            _streamsConfig = new List<StreamConfig>();
            configuration.GetSection("StreamsConfig").Bind(_streamsConfig);
        }
        public IActionResult Index(string channel)
        {
            var data = new LivePlayerView();

            data.Channel = "";
            foreach (var x in _streamsConfig)
            {
                if (channel == x.Name)
                {
                    data.Channel = channel;
                    break;
                }
            }

			return View(data);
        }
    }
}
