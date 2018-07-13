using System;
using System.Threading.Tasks;
using VideoStreamer.Models.TokenParserModels;

namespace VideoStreamer.Services.TokenParsers
{
	public interface ITokenParser
    {
		Task<TokenParseResult> ParseAsync(TokenParserModel model);
    }
}
