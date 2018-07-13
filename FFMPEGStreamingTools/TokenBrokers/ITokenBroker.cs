using System;
using FFMPEGStreamingTools.Models;

namespace FFMPEGStreamingTools.TokenBrokers
{
	public interface ITokenBroker
    {
		string GenerateToken(StreamingSession session, string salt);
    }
}
