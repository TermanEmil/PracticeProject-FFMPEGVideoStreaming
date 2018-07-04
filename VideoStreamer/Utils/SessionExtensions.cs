using System;
using FFMPEGStreamingTools;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace VideoStreamer.Utils
{
	public static class SessionExtensions
    {
		private const string _streamReqStateSessionKey = "StreamRequestState";

		public static void SetStreamRequestState(
			this ISession session,
			StreamRequestState streamRequestState)
		{
			var json = JsonConvert.SerializeObject(streamRequestState);
			session.SetString(_streamReqStateSessionKey, json);
		}

		public static StreamRequestState GetStreamRequestState(
			this ISession session)
		{
			var json = session.GetString(_streamReqStateSessionKey);

			if (json == null)
				return null;
			else
			{
				var result =
					JsonConvert.DeserializeObject(json) as StreamRequestState;
				return result;
			}
		}
    }
}
