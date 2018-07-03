using Microsoft.Extensions.Configuration;

namespace FFMPEGStreamingTools.StreamingSettings
{
	public class FFMPEGConfig
    {
		public string BinaryPath { get; set; }
		public string ChunkStorageDir { get; set; }
    }
}
