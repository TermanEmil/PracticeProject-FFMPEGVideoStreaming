using System.Collections.Generic;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VideoStreamer.Models.LivePlayerViewModels;

namespace VideoStreamer.Controllers
{
	[Route("api/[controller]/{channel}")]
    public class LivePlayerController : Controller
    {
		private readonly List<StreamSource> _streamsCfg;

		public LivePlayerController(
			StreamSourceCfgLoader streamSourceCfgLoader)
        {
			_streamsCfg = streamSourceCfgLoader.LoadStreamSources();
        }

        public IActionResult Index(string channel)
        {
            var data = new LivePlayerView();

            data.Channel = "";
            foreach (var x in _streamsCfg)
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
