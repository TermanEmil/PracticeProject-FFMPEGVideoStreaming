using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace VideoStreamer.Models.LivePlayerViewModels
{
    public class ProcessMonitorView
    {
        public Dictionary<String, Process> _StreamingProcesses { get; set; }
    }
}
