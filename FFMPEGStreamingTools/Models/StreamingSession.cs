using System;

namespace FFMPEGStreamingTools.Models
{
	public enum EStreamingSessionType
	{
        Guest, Paid, Unkown
	};

	public class StreamingSession
	{
		public string Channel { get; set; }
		public int HlsListSize { get; set; }
		public int LastFileIndex { get; set; }
		public string IP { get; set; }
		public string UserAgent { get; set; }
		public DateTime LastFileTimeSpan { get; set; }
		public EStreamingSessionType SessionType { get; set; }
        
		// Determines if the content will be downloadable.
		public bool DisplayContent { get; set; }
	}   
}
