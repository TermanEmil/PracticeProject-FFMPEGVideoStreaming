using System;
using DataLayer;

namespace FFMPEGStreamingTools.TokenBrokers
{
	public interface ITokenBroker
    {
		string GenerateToken(StreamSession session, string salt);
    }
}
