﻿using System;
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
        public string TokenSALT { get; set; }

		public static StreamerSessionCfg Load(IConfiguration cfg)
		{
			var result = cfg.GetSection("StreamingSessionsConfig")
			                .Get<StreamerSessionCfg>();
			result.CheckForEnvironmentalues();

			return result;
		}

		private void CheckForEnvironmentalues()
		{
			if (TokenSALT[0] == '$')
			{
				var envVarName = TokenSALT.Substring(1);
				TokenSALT = Environment.GetEnvironmentVariable(envVarName);
			}
		}
    }
}
