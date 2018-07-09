using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace VideoStreamer.Models.Configs
{
    public class StreamerSessionCfg
    {
		public double ExpirationTimeSeconds { get; set; }
    }
}
