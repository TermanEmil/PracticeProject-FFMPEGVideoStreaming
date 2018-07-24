using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using DataLayer.Configs;
using Shared.Logic;
using Shared.Logic.Utils;
using VideoStreamer.BusinessLogic.Models.ChunksCollectorModels;

namespace VideoStreamer.BusinessLogic.ChunksCollectors
{
	public class ChunkCollector : IChunkCollector
	{
		private const int safeHlsLstDelta = 1;

		private readonly ChunkerConfig _chunkerConfig;
		private readonly StreamsConfig _streamsConfigs;

		public ChunkCollector(
			ChunkerConfig chunkerConfig,
			StreamsConfig streamsConfigs)
		{
			_chunkerConfig = chunkerConfig;
			_streamsConfigs = streamsConfigs;
		}

		public ChunkFile GetClosestChunk(ChunksCollectorModelByTime model)
		{
			var streamCfg =
				_streamsConfigs.StreamSources
							   .FirstOrDefault(x => x.Name == model.Channel);

			if (streamCfg == null)
				throw new NoSuchChannelException(model.Channel);

			model.channelRoot = Path.Combine(
				_chunkerConfig.ChunkStorageDir,
				streamCfg.Name);
			model.streamSource = streamCfg;

			var chunkTime = streamCfg.ChunkTime;
			var minTime = GetMinChunkTimeSpan(model);
			var minTimeS = minTime
				.Add(-DateTimeOffset.Now.Offset)
				.ToUnixTimeSeconds();

			var testTime = TimeTools.SecsToDateWithOffset((int)minTimeS);

			var files = GetFilesInsideTimeRange(
				model.channelRoot,
				minTime.AddSeconds(-3.2 * 1 * chunkTime),
				minTime.AddSeconds(
					1 * chunkTime * (model.HlsListSize + safeHlsLstDelta + 1))
			);

			var result = files
				.Select(x => ChunkFileLoader.Load(x))
				.Where(x => x.timeSeconds > minTimeS - streamCfg.ChunkTime)
				.OrderBy(x => x.timeSeconds)
				.FirstOrDefault();

			if (result == null)
				throw new NoAvailableFilesException();

			return result;
		}

		public ChunkFile[] GetNextBatch(ChunksCollectorModelByLast model)
		{
			var streamCfg =
				_streamsConfigs.StreamSources
							   .FirstOrDefault(x => x.Name == model.Channel);

			if (streamCfg == null)
				throw new NoSuchChannelException(model.Channel);

			model.channelRoot = Path.Combine(
				_chunkerConfig.ChunkStorageDir,
				streamCfg.Name);
			model.streamSource = streamCfg;

			var chunkTime = streamCfg.ChunkTime;
			var lastChunk = ChunkFileLoader.Load(model.LastChunkPath);
			var lastChunkTime =
				TimeTools.SecsToDateWithOffset(lastChunk.timeSeconds);

			var files = GetFilesInsideTimeRange(
				model.channelRoot,
				lastChunkTime.AddSeconds(-chunkTime * 1),
				lastChunkTime.AddSeconds(
					chunkTime * (model.HlsListSize + safeHlsLstDelta + 1))
			);

			var minTimeS = lastChunk.timeSeconds;
			var chunks = files
				.Select(x => ChunkFileLoader.Load(x))
				.Where(x => x.timeSeconds >= minTimeS - streamCfg.ChunkTime)
				.OrderBy(x => x.timeSeconds)
				.Take(model.HlsListSize + safeHlsLstDelta)
				.ToArray();

			if (chunks.Length != model.HlsListSize + safeHlsLstDelta)
			{
				throw new NoAvailableFilesException(string.Format(
					"{0}/{1} (+{2})",
					(chunks.Length),
					(model.HlsListSize + safeHlsLstDelta),
					safeHlsLstDelta
				));
			}

			ApplyDisconinuityMarks(chunks, streamCfg.ChunkTime);
			return chunks.Skip(1).Take(model.HlsListSize).ToArray();
		}

		/// <summary>
		/// Get the closest possible time to the target time, depending
		/// on the number of chunks and the newest file.
		/// If it's live, then the newest file - (n + 1) * chunkTime will
		/// be returned. Note +1 because the newest is still being created.
		/// 
		/// Otherwise, the received time is returned.
		/// </summary>
		private DateTime GetMinChunkTimeSpan(ChunksCollectorModelByTime model)
		{
			var mostRecent =
				Directory.GetFiles(
					model.channelRoot,
					"*.ts",
					SearchOption.AllDirectories
				).OrderByDescending(File.GetCreationTime).FirstOrDefault();

			if (mostRecent == null)
				throw new NoAvailableFilesException($"0/{model.HlsListSize}");

			var targetTime = model.RequestedTime;
			var newestChunk = ChunkFileLoader.Load(mostRecent);
			var newestDateTime =
				TimeTools.SecsToDateWithOffset(newestChunk.timeSeconds);

			var totalRequiredSec =
				(model.HlsListSize + safeHlsLstDelta + 1) *
				(model.streamSource.ChunkTime);

			if (targetTime.AddSeconds(totalRequiredSec) > newestDateTime)
			{
				var delta = newestDateTime - targetTime;
				return newestDateTime.AddSeconds(-totalRequiredSec)
									 .Add(-delta);
			}
			else
			{
				return targetTime;
			}
		}

		private static void ApplyDisconinuityMarks(
			ChunkFile[] chunks,
			double chunkTime)
		{
			for (int i = 1; i < chunks.Length; i++)
			{
				if ((chunks[i].index - 1 != chunks[i - 1].index) ||
					TimeDistTooBig(chunks[i], chunks[i - 1], chunkTime))
				{
					chunks[i].isDiscont = true;
					chunks[i - 1].isDiscont = true;
				}
			}
		}

		#region Helpers
		private static List<string> GetFilesInsideTimeRange(
            string root,
            DateTime minTime,
            DateTime maxTime)
        {
            var result = new List<string>();
            for (var time = minTime; time <= maxTime; time = time.AddMinutes(1))
            {
				var dir = root + "/" + time.GetDir();
				if (Directory.Exists(dir))
				{
					result.AddRange(Directory.GetFiles(dir));
				}
            }

            return result;
        }

		private static bool TimeDistTooBig(
			ChunkFile chunk1,
			ChunkFile chunk2,
			double chunkTime)
		{
			var delta = Math.Abs(chunk1.timeSeconds - chunk2.timeSeconds);
			return delta > chunkTime * 1.5;
		}
		#endregion
	}
}
