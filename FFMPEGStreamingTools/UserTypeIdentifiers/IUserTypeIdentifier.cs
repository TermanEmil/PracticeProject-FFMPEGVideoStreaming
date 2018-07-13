using System;
using FFMPEGStreamingTools.Models;

namespace FFMPEGStreamingTools.StreamingSessionTypeIdentifiers
{
	public interface IUserTypeIdentifier
    {
		EStreamingSessionType GetSessionType(SessionBrokerModel model);
    }
}
