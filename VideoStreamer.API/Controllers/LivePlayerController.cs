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
		private readonly StreamSourceCfgLoader _streamSourceCfgLoader;

		public LivePlayerController(
			StreamSourceCfgLoader streamSourceCfgLoader)
        {
            _streamSourceCfgLoader = streamSourceCfgLoader;
        }

        public IActionResult Index(string channel)
        {
			var streamsCfg = _streamSourceCfgLoader.LoadStreamSources();
			if (streamsCfg == null)
				return NotFound();

			var data = new LivePlayerView { Channel = "" };

            foreach (var x in streamsCfg)
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
