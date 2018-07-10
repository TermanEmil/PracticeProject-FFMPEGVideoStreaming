using Microsoft.Extensions.Configuration;

namespace FFMPEGStreamingTools.StreamingSettings
{
	public class FFMPEGConfig
	{      
		public string BinaryPath { get; set; }
		public string ChunkStorageDir { get; set; }
		public string ChannelsCfgPath { get; set; }

		public static FFMPEGConfig Load(IConfiguration cfg)
        {
            return cfg.GetSection(typeof(FFMPEGConfig).Name)
                      .Get<FFMPEGConfig>();
        }
	}
}
