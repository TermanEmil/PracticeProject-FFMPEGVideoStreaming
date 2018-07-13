using System;
using FFMPEGStreamingTools.Models;
using FFMPEGStreamingTools.StreamingSessionTypeIdentifiers;

namespace FFMPEGStreamingTools.UserTypeIdentifiers
{
	public class PaidUserTypeIdentifier : IUserTypeIdentifier
    {      
		public EStreamingSessionType GetSessionType(SessionBrokerModel model)
		{
			var regToken = model.RegistrationToken;

            if (regToken != null)
            {
                if (regToken.StartsWith("Unicorn", StringComparison.Ordinal))
                    return EStreamingSessionType.Paid;
            }

			return EStreamingSessionType.Unkown;
		}
	}
}
