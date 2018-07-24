using System;
using DataLayer;
using FFMPEGStreamingTools.Models;

namespace FFMPEGStreamingTools.SessionBrokers
{
	public interface ISessionBroker
    {
		StreamSession InitializeSession(SessionBrokerModel model);
    }
}
