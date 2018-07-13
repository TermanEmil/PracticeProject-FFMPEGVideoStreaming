using System;
using System.Threading.Tasks;
using FFMPEGStreamingTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using VideoStreamer.Models.TokenParserModels;
using VideoStreamer.Utils;

namespace VideoStreamer.Services.TokenParsers
{   
	public class TokenParser : ITokenParser
    {
		private readonly IDistributedCache _cache;

		public TokenParser(IDistributedCache cache)
		{
			_cache = cache;
		}

		public async Task<TokenParseResult> ParseAsync(TokenParserModel model)
		{
			while (true)
            {
                if (model.Token == null)
                    break;

                StreamingSession session;
                try
                {
                    session = await
						_cache.GetAsync<StreamingSession>(model.Token);
                }
                catch (Exception)
                {
					return new TokenParseResult(null, new ViewResult()
				    {
					    ViewName = "RedisConnectionException"
				    });
                }

                if (session == null)
                    break;

				if (model.Channel != session.Channel)
                    break;

				var ip = model.HttpContext
				              .Connection
				              .RemoteIpAddress
				              .ToString();
				
                if (session.IP != ip)
                {
					return new TokenParseResult(
						null,
						new UnauthorizedResult());
                }

				return new TokenParseResult(session, null);
            }

			return new TokenParseResult(null, new NotFoundResult());
		}
    }
}
