using FFMPEGStreamingTools;
using Microsoft.AspNetCore.Mvc;
using VideoStreamer.Models.LivePlayerViewModels;

namespace VideoStreamer.Controllers
{
	[Route("api/[controller]")]
    public class ProcessMonitorController : Controller
    {
        public IActionResult Index()
        {

            var ProcessData = new ProcessMonitorView
            {
                _StreamingProcesses = StreamingProcManager.instance.processes,
            };
            return View(ProcessData);
        }
    }
}
