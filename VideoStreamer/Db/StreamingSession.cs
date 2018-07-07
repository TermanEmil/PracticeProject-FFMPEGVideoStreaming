using System;
namespace VideoStreamer.DB
{
    public class StreamingSession
    {
		public string ID { get; set; }
		public string Channel { get; set; }
		public string LastFilePath { get; set; }
		public DateTime ExpireTime { get; set; }
    }
}
