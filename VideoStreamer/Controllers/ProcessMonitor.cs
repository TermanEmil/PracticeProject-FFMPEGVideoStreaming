using Microsoft.AspNetCore.Mvc;
using VideoStreamer.Models.LivePlayerViewModels;
using FFMPEGStreamingTools;
using System.Diagnostics;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.Net;

namespace VideoStreamer.Controllers
{
    [Route("api/[controller]")]
    public class ProcessMonitor : Controller
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
