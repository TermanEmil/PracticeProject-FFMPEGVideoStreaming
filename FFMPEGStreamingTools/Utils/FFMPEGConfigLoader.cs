using System;
using System.Collections.Generic;
using FFMPEGStreamingTools.StreamingSettings;
using Microsoft.Extensions.Configuration;

namespace FFMPEGStreamingTools.Utils
{
	public static class FFMPEGConfigLoader
    {
		public static void Load(
			IConfiguration cfg,
			out FFMPEGConfig ffmpegCfg,
			out List<StreamConfig> streamsConfigs)
		{
			ffmpegCfg = cfg.GetSection("FFMPEGConfig")
			               .Get<FFMPEGConfig>();
			
			streamsConfigs = new List<StreamConfig>();
			cfg.GetSection("StreamsConfig")
			   .Bind(streamsConfigs);
		}
    }
}
