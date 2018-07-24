using System;
using DataLayer;
using Microsoft.AspNetCore.Mvc;

namespace VideoStreamer.Models.TokenParserModels
{
	public struct TokenParseResult
    {
        public StreamSession session;
        public IActionResult actionResult;

		public TokenParseResult(StreamSession session, IActionResult ac)
        {
            this.session = session;
            this.actionResult = ac;
        }
    }
}
