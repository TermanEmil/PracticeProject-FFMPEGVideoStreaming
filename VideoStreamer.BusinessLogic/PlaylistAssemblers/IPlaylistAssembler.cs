using System.Collections.Generic;
using DataLayer;
using DataLayer.Configs;
using VideoStreamer.BusinessLogic.Models;

namespace VideoStreamer.BusinessLogic.PlaylistAssemblers
{
    public interface IPlaylistAssembler
    {
		M3U8Playlist Aseemble(
			StreamSession streamSession,
			IEnumerable<ChunkFile> chunks);
    }
}