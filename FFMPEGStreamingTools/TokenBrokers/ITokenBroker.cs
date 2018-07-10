using System;
namespace FFMPEGStreamingTools.TokenBrokers
{
	public interface ITokenBroker
    {
		string GenerateToken(string someStrData, string salt);
    }
}
