using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using DataLayer.Configs;
using VideoStreamer.BusinessLogic.Models;

namespace VideoStreamer.BusinessLogic.PlaylistAssemblers
{
    public class SimplePlaylistAssembler : IPlaylistAssembler
	{
		private readonly ChunkerConfig _chunkerConfig;
		private readonly StreamsConfig _streamsConfig;

		public SimplePlaylistAssembler(
		    ChunkerConfig chunkerConfig,
			StreamsConfig streamsConfig)
        {
			_chunkerConfig = chunkerConfig;
			_streamsConfig = streamsConfig;
        }

		public M3U8Playlist Aseemble(
			StreamSession streamSession,
			IEnumerable<ChunkFile> chunks)
		{
			var streamCfg =
				_streamsConfig.StreamSources
							  .First(x => x.Name == streamSession.Channel);
		             
			var root = Path.Combine(
				_chunkerConfig.ChunkStorageDir,
				streamCfg.Name);

			return new M3U8Playlist
			{
				Headers = new[]
				{
					"#EXTM3U",
					"#EXT-X-VERSION:3",
					$"#EXT-X-TARGETDURATION:{streamCfg.ChunkTime}",
					//$"EXT-X-DISCONTINUITY-S   EQUENCE:{streamSession.DiscontSeq}",
					$"#EXT-X-MEDIA-SEQUENCE:{streamSession.MediaSeq}",               
				},
				Files = chunks.Select(file => new M3U8FileEntry
				{
					Extinf = streamCfg.ChunkTime,
					FilePath = file.fullPath.Replace(root, ""),
					IsDiscont = file.isDiscont
				}).ToArray()
			};
		}
    }
}
