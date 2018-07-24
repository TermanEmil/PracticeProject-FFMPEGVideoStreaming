using System;
using System.Collections.Generic;
using DataLayer;
using DataLayer.Configs;

namespace VideoStreamer.BusinessLogic.Models
{
	public class PlaylistAssemblerModel
	{
		public StreamSession Session { get; set; }
		public StreamSource StreamCfg { get; set; }
		public IEnumerable<ChunkFile> Chunks { get; set; }
    }
}
