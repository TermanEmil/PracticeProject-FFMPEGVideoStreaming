using System;
namespace FFMPEGStreamingTools.Models
{
    public class SessionBrokerModel
    {
		public string Channel { get; set; }
		public DateTime RequiredTime { get; set; }
		public int ListSize { get; set; }
		public bool DisplayContent { get; set; }
		public string RegistrationToken { get; set; }
		public string IP { get; set; }
		public string UserAgent { get; set; }
    }
}
