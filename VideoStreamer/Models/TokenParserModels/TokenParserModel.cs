using System;
using Microsoft.AspNetCore.Http;

namespace VideoStreamer.Models.TokenParserModels
{
	public class TokenParserModel
    {
		public HttpContext HttpContext { get; set; }
		public string Token { get; set; }
		public string Channel { get; set; }
    }
}
