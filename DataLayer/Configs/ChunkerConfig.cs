using System;
using Microsoft.Extensions.Configuration;

namespace DataLayer.Configs
{
    public class ChunkerConfig
    {
		public string BinaryPath { get; set; }
        public string ChunkStorageDir { get; set; }
        public string ChannelsCfgPath { get; set; }

		public static ChunkerConfig Load(IConfiguration cfg)
        {
			return cfg.GetSection(typeof(ChunkerConfig).Name)
				      .Get<ChunkerConfig>();
        }
    }
}
