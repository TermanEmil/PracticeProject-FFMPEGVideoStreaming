using System;
namespace FFMPEGStreamingTools.StreamingSettings
{
    public class StreamSource
    {
		public string Link { get; set; }
		public string Name { get; set; }
		public double ChunkTime { get; set; }

		public override int GetHashCode()
		{
			return
				(Link.GetHashCode()) ^
				(Name.GetHashCode()) ^
				(ChunkTime.GetHashCode());
		}
	}
}
