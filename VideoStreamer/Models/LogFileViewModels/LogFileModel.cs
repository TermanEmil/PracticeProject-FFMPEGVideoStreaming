using System;

namespace VideoStreamer.Models.LogFileViewModels
{
    public class LogFileModel
    {
        public string Channel { get; set; }
        public int HlsListSize { get; set; }
        public int Interval { get; set; }
    }
}