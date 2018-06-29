using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProcessStreamer;
using VideoStreamer.Models.ProcMonitorViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VideoStreamer.Controllers
{
	[Route("api/[controller]")]
    public class ProcMonitorController : Controller
    {
        public IActionResult Index()
        {         
			var data = new ProcMonitorViewModel
			{
				SomeData = StreamingProcManager.instance.processes[0].ProcessName
			};

			return View(data);
        }
    }
}
