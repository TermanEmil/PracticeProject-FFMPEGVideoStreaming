using System;
using FFMPEGStreamingTools.Models;

namespace FFMPEGStreamingTools.SessionBrokers
{
	public interface ISessionBroker
    {
		StreamingSession InitializeSession(SessionBrokerModel model);
    }
}
