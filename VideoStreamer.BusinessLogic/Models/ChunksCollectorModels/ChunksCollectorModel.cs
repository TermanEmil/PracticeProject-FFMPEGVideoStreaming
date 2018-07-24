using System;
using DataLayer.Configs;

namespace VideoStreamer.BusinessLogic.Models.ChunksCollectorModels
{
    public class ChunksCollectorModelBase
    {
		public string Channel { get; set; }
		public int HlsListSize { get; set; }

		internal string channelRoot;
		internal StreamSource streamSource;
    }

	public class ChunksCollectorModelByTime : ChunksCollectorModelBase
	{
		public DateTime RequestedTime { get; set; }
	}

	public class ChunksCollectorModelByLast : ChunksCollectorModelBase
	{
		public string LastChunkPath { get; set; }
	}
}
