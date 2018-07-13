using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataLayer.Configs
{
    public class StreamsConfig
    {
		private List<StreamSource> streamSources = new List<StreamSource>();
		private readonly ChunkerConfig _chunkerConfig;
		private readonly ILogger<StreamsConfig> _logger;

		public IReadOnlyList<StreamSource> StreamSources
		    => streamSources.AsReadOnly();

		public StreamsConfig(
			ChunkerConfig chunkerConfig,
			ILogger<StreamsConfig> logger)
		{
			_chunkerConfig = chunkerConfig;
			_logger = logger;
		}

		public void Reload(string channelsCfgPath = null)
		{
			if (channelsCfgPath == null)
				channelsCfgPath = _chunkerConfig.ChannelsCfgPath;
			
			var jsonFile = File.ReadAllText(channelsCfgPath);

			List<StreamSource> newSources;
            try
            {
				newSources = 
                    JsonConvert.DeserializeObject<List<StreamSource>>(jsonFile);
            }
            catch (JsonException)
            {
				_logger.LogError("Invalid Streaming Source json format");
                return;
            }

			streamSources = newSources;
		}
    }
}
