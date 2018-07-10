using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FFMPEGStreamingTools.TokenBrokers
{
	public class SHA256TokenBroker : ITokenBroker
	{
		public string GenerateToken(string someStrData, string salt)
		{
			var strToHash = someStrData + DateTime.Now + salt;         
            return SHA256Encrypt(strToHash);
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
