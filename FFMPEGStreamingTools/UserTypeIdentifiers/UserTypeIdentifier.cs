using System;
using System.Collections.Generic;
using FFMPEGStreamingTools.Models;
using FFMPEGStreamingTools.StreamingSessionTypeIdentifiers;

namespace FFMPEGStreamingTools.UserTypeIdentifiers
{
    public class UserTypeIdentifier
    {
		private readonly IEnumerable<IUserTypeIdentifier> _userTypeIdentifiers;

		public UserTypeIdentifier(
			IEnumerable<IUserTypeIdentifier> userTypeIdentifiers)
        {
			_userTypeIdentifiers = userTypeIdentifiers;
        }

		public EStreamingSessionType Identify(SessionBrokerModel model)
		{
			foreach (var identifier in _userTypeIdentifiers)
			{
				var result = identifier.GetSessionType(model);
				if (result != EStreamingSessionType.Unkown)
					return result;
			}

			return EStreamingSessionType.Unkown;
		}
    }
}
