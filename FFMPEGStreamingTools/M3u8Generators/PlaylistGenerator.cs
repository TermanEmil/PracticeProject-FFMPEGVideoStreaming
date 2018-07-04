using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MoreLinq;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;

namespace FFMPEGStreamingTools.M3u8Generators
{   
	public class PlaylistGenerator
    {
		// This number indicates how many extra chunks there are.
		// In total, there will be at least hlsListSize + (this number) chunks.
		private readonly int safeHlsLstDelta = 1;

		public string GenerateM3U8Str(
			string channel,
			DateTime time,
			FFMPEGConfig ffmpegCfg,
			IEnumerable<StreamConfig> streamsCfgs,
			ref StreamRequestState streamRequestState,
			int hlsLstSize = 5)
		{
			var streamCfg = streamsCfgs.FirstOrDefault(x => x.Name == channel);
			if (streamCfg == null)
				throw new NoSuchChannelException(streamCfg.Name);

			var channelRoot = $"{ffmpegCfg.ChunkStorageDir}/{channel}/";

			var fileChunks = GatherRequiredChunks(
				streamCfg,
				time,
				hlsLstSize,
				channelRoot);

			return CombineChunksIntoM3U8(
				streamCfg,
				fileChunks,
				channelRoot,
				channel);
		}

		private IEnumerable<ChunkFile> GatherRequiredChunks(
			StreamConfig streamCfg,
			DateTime time,
		    int hlsLstSize,
			string channelRoot)
		{
			var targetTime = GetMinChunkTimeSpan(
                hlsLstSize + safeHlsLstDelta,
                streamCfg.ChunkTime,
                time,
                channelRoot);

            var targetTimeS = targetTime.Add(-DateTimeOffset.Now.Offset)
                                        .ToUnixTimeSeconds();

			var files = Directory.GetFiles(
				channelRoot,
				"*.ts",
				SearchOption.AllDirectories);
			
            var chunks = 
				files.Select(x => new ChunkFile(x))
                     .Where(x =>
				            x.timeSeconds >= targetTimeS - streamCfg.ChunkTime)
				     .OrderBy(x => x.timeSeconds)
                     .Take(hlsLstSize + safeHlsLstDelta)
                     .ToArray();

            if (chunks.Length != hlsLstSize + safeHlsLstDelta)
            {
                throw new NoAvailableFilesException(
                    chunks.Length,
                    hlsLstSize + safeHlsLstDelta,
                    $"(+{safeHlsLstDelta})");
            }

            var fileChunks = GetConsecutiveChunks(chunks).Take(hlsLstSize)
                                                         .ToArray();

            if (fileChunks.Length != hlsLstSize)
            {
                throw new NoAvailableFilesException(
                    fileChunks.Length,
                    hlsLstSize);
            }

			return fileChunks;
		}

		private string CombineChunksIntoM3U8(
			StreamConfig streamCfg,
			IEnumerable<ChunkFile> fileChunks,
			string channelRoot,
			string channel)
		{
			var content = String.Join("\n", new[]
            {
                "#EXTM3U",
                "#EXT-X-VERSION:3",
                $"#EXT-X-TARGETDURATION:{streamCfg.ChunkTime}",
				$"#EXT-X-MEDIA-SEQUENCE:{fileChunks.First().index}",
            });
            content += ",\n";

			foreach (var file in fileChunks)
            {
				var isDiscont =
					StreamingProcManager.instance
					                    .chunkDiscontinuities[channel]
					                    .Contains(file.index);
				
				if (isDiscont)
                    content += "#EXT-X-DISCONTINUITY\n";

                content += $"#EXTINF:{streamCfg.ChunkTime},\n";
                content += file.fullPath.Replace(channelRoot, "") + "\n";
            }

            return content;
		}

		/// <summary>
        /// Get the closest possible time to the target time, depending
        /// on the number of chunks and the newest file.
        /// If it's live, then the newest file - (n + 1) * chunkTime will
        /// be returned. Note +1 because the newest is still being created.
        /// 
        /// Otherwise, the received time is returned.
        /// </summary>
        private DateTime GetMinChunkTimeSpan(
            int chunksCount,
            double chunkTime,
            DateTime targetTime,
            string chunksRoot)
        {
            var mostRecent =
                Directory.GetFiles(chunksRoot, "*.ts", SearchOption.AllDirectories)
                         .OrderByDescending(File.GetLastWriteTime)
                         .First();

            var newestChunk = new ChunkFile(mostRecent);
            var newestDateTime = TimeTools
                .SecondsToDateTime(newestChunk.timeSeconds)
                .Add(DateTimeOffset.Now.Offset);

            var totalRequiredSec = (chunksCount + 1) * chunkTime;
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

        /// <summary>
        /// Returns only the first chunks that have a consecutive index.
        /// </summary>
        /// <returns>The continuous chunks.</returns>
        /// <param name="chunks">Chunks.</param>
		private IEnumerable<ChunkFile> GetConsecutiveChunks(
            IEnumerable<ChunkFile> chunks)
        {
            ChunkFile lastChunk = null;
            foreach (var chunk in chunks)
            {
                if (lastChunk != null && chunk.index - lastChunk.index != 1)
                    yield break;
                else
                    yield return chunk;
                lastChunk = chunk;
            }
        }

		private string GetDirOfDateTime(DateTime time)
		{
			return
				$"{time.Year}/" +
				$"{time.Month.ToString("00")}/" +
				$"{time.Day.ToString("00")}/" +
				$"{time.Hour.ToString("00")}/" +
				$"{time.Minute.ToString("00")}/";
		}
	}
}
