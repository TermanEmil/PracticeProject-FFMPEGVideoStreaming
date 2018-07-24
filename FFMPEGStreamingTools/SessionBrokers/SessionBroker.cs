using System;
using System.Linq;
using DataLayer;
using FFMPEGStreamingTools.Models;
using FFMPEGStreamingTools.UserTypeIdentifiers;
using FFMPEGStreamingTools.Utils;
using VideoStreamer.BusinessLogic.ChunksCollectors;
using VideoStreamer.BusinessLogic.Models.ChunksCollectorModels;
using VideoStreamer.BusinessLogic.PlaylistAssemblers;

namespace FFMPEGStreamingTools.SessionBrokers
{
	public class SessionBroker : ISessionBroker
    {
		private readonly IChunkCollector _chunkCollector;
		private readonly UserTypeIdentifier _userTypeIdentifier;

		public SessionBroker(
			IChunkCollector chunkCollector,
			UserTypeIdentifier userTypeIdentifier)
		{
			_chunkCollector = chunkCollector;
			_userTypeIdentifier = userTypeIdentifier;
		}

		public StreamSession InitializeSession(SessionBrokerModel model)
		{
			var closestChunk = _chunkCollector.GetClosestChunk(
				new ChunksCollectorModelByTime
				{
					Channel = model.Channel,
					HlsListSize = model.ListSize,
					RequestedTime = model.RequiredTime
				}
			);

			return new StreamSession
			{
				Channel = model.Channel,
				HlsListSize = model.ListSize,
				IP = model.IP,
				UserAgent = model.UserAgent,
				DisplayContent = model.DisplayContent,
				MediaSeq = 0,
				DiscontSeq = 0,
				LastFilePath = closestChunk.fullPath
            };
		}
    }
}
