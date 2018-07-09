using System;
using System.Collections.Generic;
using FFMPEGStreamingTools.StreamingSettings;

namespace FFMPEGStreamingTools.M3u8Generators
{
	public interface IM3U8Generator
    {
		M3U8Playlist GenerateM3U8(
			FFMPEGConfig ffmpegCfg,
            IEnumerable<StreamConfig> streamsCfgs,
			string channel,
			DateTime time,         
			int hlsLstSize);

		M3U8Playlist GenerateNextM3U8(
			FFMPEGConfig ffmpegCfg,
			IEnumerable<StreamConfig> streamsCfgs,
			string channel,
			int hlsLstSize,
			int lastFileIndex,
			DateTime lastFileTimeSpan);
    }
}
