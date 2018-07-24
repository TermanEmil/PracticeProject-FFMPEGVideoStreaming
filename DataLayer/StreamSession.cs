using System;

namespace DataLayer
{
    public class StreamSession
    {
		public string Channel { get; set; }
        public int HlsListSize { get; set; }
		public string LastFilePath { get; set; }
		public string IP { get; set; }
        public string UserAgent { get; set; }

        // Playlist specific
		public int MediaSeq { get; set; }
		public int DiscontSeq { get; set; }

        // Determines if the content will be downloadable.
        public bool DisplayContent { get; set; }
    }
}
