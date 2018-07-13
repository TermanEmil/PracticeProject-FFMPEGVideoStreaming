using System;
using FFMPEGStreamingTools.Models;
using Microsoft.AspNetCore.Mvc;

namespace VideoStreamer.Models.TokenParserModels
{
	public struct TokenParseResult
    {
        public StreamingSession session;
        public IActionResult actionResult;

        public TokenParseResult(StreamingSession session, IActionResult ac)
        {
            this.session = session;
            this.actionResult = ac;
        }
    }
}
