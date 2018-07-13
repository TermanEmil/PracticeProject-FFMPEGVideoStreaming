using System;
using FFMPEGStreamingTools.Models;
using FFMPEGStreamingTools.StreamingSessionTypeIdentifiers;

namespace FFMPEGStreamingTools.UserTypeIdentifiers
{
	public class GuestUserTypeIdentifier : IUserTypeIdentifier
    {      
		public EStreamingSessionType GetSessionType(SessionBrokerModel model)
		{
			return EStreamingSessionType.Guest;
		}
	}
}
