using System;
using FFMPEGStreamingTools.Models;

namespace FFMPEGStreamingTools.StreamingSessionTypeIdentifiers
{
	public class StreamingSessionTypeIdentifier
		: IStreamingSessionTypeIdentifier
    {      
		public EStreamingSessionType GetSessionType(SessionBrokerModel model)
		{
			var regToken = model.RegistrationToken;

			if (regToken != null)
			{
				if (regToken.StartsWith("Unicorn", StringComparison.Ordinal))
					return EStreamingSessionType.Paid;
			}

			return EStreamingSessionType.Guest;
		}
	}
}
