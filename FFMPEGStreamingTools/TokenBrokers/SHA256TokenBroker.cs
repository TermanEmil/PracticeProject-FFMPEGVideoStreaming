using System;
using System.Security.Cryptography;
using System.Text;
using FFMPEGStreamingTools.Models;

namespace FFMPEGStreamingTools.TokenBrokers
{
	public class SHA256TokenBroker : ITokenBroker
	{
		public string GenerateToken(StreamingSession session, string salt)
		{
			var strToHash = string.Join("", new[]
			{
				session.Channel,
				session.IP,
				session.UserAgent,
				DateTime.Now.ToString(),
				salt
			});

			return String.Join("", new[]
			{
				session.SessionType.ToString(),
                "_",
				SHA256Encrypt(strToHash)
			});
		}

		private static string SHA256Encrypt(string phrase)
        {
            var sha256hasher = new SHA256Managed();
            var hashedDataBytes = sha256hasher.ComputeHash(
                Encoding.UTF8.GetBytes(phrase));

            return Convert.ToBase64String(hashedDataBytes)
                          .Replace("+", "-")
                          .Replace("/", "_");
        }
	}
}
