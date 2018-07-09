using System;

namespace VideoStreamer.DB
{
    public class StreamingSession
    {
		public string ID { get; set; }
		public string Channel { get; set; }
		public int HlsListSize { get; set; }
		public int LastFileIndex { get; set; }
		public string IP { get; set; }
		public DateTime LastFileTimeSpan { get; set; }
		public DateTime ExpireTime { get; set; }

        // Determines if the content will be downloadable.
		public bool DisplayContent { get; set; }
    }
}
