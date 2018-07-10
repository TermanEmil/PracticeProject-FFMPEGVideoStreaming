using System;
using System.Collections.Generic;
using System.IO;
using FFMPEGStreamingTools.StreamingSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FFMPEGStreamingTools.Utils
{
	public class StreamSourceCfgLoader
    {
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly ILogger<StreamSourceCfgLoader> _logger;

		public StreamSourceCfgLoader(
			IConfiguration cfg,
			ILogger<StreamSourceCfgLoader> logger)
		{
			_ffmpegCfg = FFMPEGConfig.Load(cfg);
			_logger = logger;
		}

		public List<StreamSource> LoadStreamSources()
		{
			var jsonFile = File.ReadAllText(_ffmpegCfg.ChannelsCfgPath);

            try
            {
				return
					JsonConvert.DeserializeObject<List<StreamSource>>(jsonFile);
            }
            catch (JsonException)
            {
				_logger.LogWarning("Invalid Streaming Source json format");
				return null;
            }
		}      
    }
}
