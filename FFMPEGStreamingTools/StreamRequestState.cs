using System;
namespace FFMPEGStreamingTools
{
    public class StreamRequestState
    {
		public DateTime ReferenceTime { get; set; }
		public string Channel { get; set; }

		public override int GetHashCode()
		{
			return
				base.GetHashCode() ^
				    ReferenceTime.GetHashCode() ^
				    Channel.GetHashCode();
		}
	}
}
