using System;
using System.Linq;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.Models;
using FFMPEGStreamingTools.UserTypeIdentifiers;
using FFMPEGStreamingTools.Utils;

namespace FFMPEGStreamingTools.SessionBrokers
{
	public class SessionBroker : ISessionBroker
    {
		private readonly IM3U8Generator _m3u8Generator;
		private readonly UserTypeIdentifier _userTypeIdentifier;

		public SessionBroker(
			IM3U8Generator m3u8Generator,
			UserTypeIdentifier userTypeIdentifier)
		{
			_m3u8Generator = m3u8Generator;
			_userTypeIdentifier = userTypeIdentifier;
		}

		public StreamingSession InitializeSession(SessionBrokerModel model)
		{
			var playlist = _m3u8Generator.GenerateM3U8(
                model.Channel,
				model.RequiredTime,
				model.ListSize);

            var firstFile = new ChunkFile(playlist.files.First().filePath);

            var lastFileTime =
                TimeTools.SecondsToDateTime(firstFile.timeSeconds)
                         .Add(DateTimeOffset.Now.Offset);
            
			return new StreamingSession
			{
				Channel = model.Channel,
				HlsListSize = model.ListSize,
				IP = model.IP,
				UserAgent = model.UserAgent,
				LastFileIndex = firstFile.index - 1,
				LastFileTimeSpan = lastFileTime,
				DisplayContent = model.DisplayContent,
				SessionType = _userTypeIdentifier.Identify(model)
            };
		}
    }
}
