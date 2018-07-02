using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProcessStreamer;
using VideoStreamer.Models.LivePlayerViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VideoStreamer.Controllers
{
    [Route("api/[controller]/{chanel}")]
    public class LivePlayerController : Controller
    {
        public IActionResult Index(string chanel)
        {         
            var data = new LivePlayerView
			{
				SomeData = StreamingProcManager.instance.processes[0].ProcessName
			};

			return View(data);
        }
    }
}
