using System;
using Microsoft.Extensions.Caching.Distributed;

namespace VideoStreamer.DB
{
	public class StreamingSession
	{
		public string Channel { get; set; }
		public int HlsListSize { get; set; }
		public int LastFileIndex { get; set; }
		public string ConnectionDetails { get; set; }
		public DateTime LastFileTimeSpan { get; set; }

		// Determines if the content will be downloadable.
		public bool DisplayContent { get; set; }
	}   
}
