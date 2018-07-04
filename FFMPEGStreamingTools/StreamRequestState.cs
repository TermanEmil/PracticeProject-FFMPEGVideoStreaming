using System;
namespace FFMPEGStreamingTools
{
    public class StreamRequestState
    {
		public DateTime ReferenceTime { get; set; }
		public string Channel { get; set; }
		public TimeSpan TimeDifference { get; set; }
	}
}
