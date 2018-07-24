using System.Linq;
using FFMPEGStreamingTools;
using Microsoft.AspNetCore.Mvc;
using VideoStreamer.Models.LivePlayerViewModels;

namespace VideoStreamer.Controllers
{
	[Route("api/[controller]")]
    public class ProcessMonitorController : Controller
    {
		private readonly StreamingProcManager _procManager;

		public ProcessMonitorController(StreamingProcManager procManager)
		{
			_procManager = procManager;
		}

        public IActionResult Index()
        {
			var ProcessData = new ProcessMonitorView
			{
				_StreamingProcesses =
					_procManager.processes
					            .ToDictionary(x => x.Name, x => x.Proc)
            };
            return View(ProcessData);
        }
    }
}
