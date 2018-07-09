using System;
using System.Collections.Generic;
using System.Linq;

namespace FFMPEGStreamingTools.M3u8Generators
{
	public struct M3U8File
	{
		public double extinf;
		public bool isDiscont;
		public string filePath;
		public int fileIndex;

		public string Bake(string fileWebArguments = null)
		{
			var argument =
				(fileWebArguments == null) ? "" : "?" + fileWebArguments;

			return (isDiscont ? "#EXT-X-DISCONTINUITY\n" : "") +
				(string.Format("#EXTINF:{0:0.0000},\n", extinf)) +
				(filePath + argument + "\n");
		}
	}

    public class M3U8Playlist
    {
		public IEnumerable<string> headers;
		public IEnumerable<M3U8File> files;

		public string Bake(string fileWebArguments = null)
		{
			return
				(String.Join("\n", headers) + ",\n") +
				(String.Join(
					"",
					files.Select(x => x.Bake(fileWebArguments))) + "\n");      
		}
    }
}
