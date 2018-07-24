using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;

namespace VideoStreamer.BusinessLogic.Models
{
	public class M3U8FileEntry
	{
		public double Extinf { get; set; }
		public string FilePath { get; set; }
		public bool IsDiscont { get; set; }

		public string Bake(string fileWebArguments = null)
        {
            var argument =
				(fileWebArguments == null) ? "" : $"?{fileWebArguments}";

			var path = FilePath;
			if (path[0] == '/')
				path = path.Substring(1);
            return
				//(IsDiscont ? "#EXT-X-DISCONTINUITY\n" : "") +
				$"#EXTINF:{Extinf:0.0000},\n" +
				$"{path}{argument}\n";
        }
	}

    public class M3U8Playlist
    {
		public IEnumerable<string> Headers { get; set; }
		public IEnumerable<M3U8FileEntry> Files { get; set; }

		public string Bake(string fileWebArguments = null)
        {
			return
				(String.Join("\n", Headers) + ",\n") +
				String.Join("", Files.Select(x => x.Bake(fileWebArguments))) +
			    "\n";
        }
    }
}
