using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;

namespace FFMPEGStreamingTools.M3u8Generators
{   
	public class M3U8GeneratorDefault : IM3U8Generator
	{
		// This number indicates how many extra chunks there are.
        // In total, there will be at least hlsListSize + (this number) chunks.
        // It helps preventing some glitches when the process dies.
        private const int safeHlsLstDelta = 1;

        // I don't really know why I've set it.
		private const double maxConnectionLatencySeconds = 5 * 60;

		public M3U8Playlist GenerateM3U8(
			FFMPEGConfig ffmpegCfg,
			IEnumerable<Channel> streamsCfgs,
			string channel,
			DateTime time,
			int hlsLstSize)
		{
			BasicInitializations(
				ffmpegCfg,
				streamsCfgs,
				channel,
				out var streamCfg,
				out var channelRoot
			);

			var chunkTime = streamCfg.ChunkTime;

			var targetTime = GetMinChunkTimeSpan(
                hlsLstSize + safeHlsLstDelta,
                chunkTime,
                time,
                channelRoot);
			
			var stopwatch = new Stopwatch();
            stopwatch.Start();

            var files = GetFilesInsideTimeRange(
                channelRoot,
                targetTime.AddSeconds(-1.2 * chunkTime),
				targetTime.AddSeconds(
					maxConnectionLatencySeconds +
					chunkTime * (hlsLstSize + safeHlsLstDelta + 1))
			);

            stopwatch.Stop();
            //Console.WriteLine("[Stopwatch]: DirectoryGetFiles:" +
                              //$"{stopwatch.ElapsedMilliseconds}");

            var fileChunks = GatherRequiredChunks(
				chunkTime,
				files,
				targetTime,
                hlsLstSize);

			return CombineChunksIntoPlaylist(
				channel,
				channelRoot,
				chunkTime,
				fileChunks);
		}

		public M3U8Playlist GenerateNextM3U8(
			FFMPEGConfig ffmpegCfg,
			IEnumerable<Channel> streamsCfgs,
			string channel,
			int hlsLstSize,
			int lastFileIndex,
			DateTime lastFileTimeSpan)
		{
			BasicInitializations(
                ffmpegCfg,
                streamsCfgs,
                channel,
                out var streamCfg,
                out var channelRoot
            );

            var chunkTime = streamCfg.ChunkTime;         
			var targetTime = lastFileTimeSpan;
             
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var files = GetFilesInsideTimeRange(
                channelRoot,
				targetTime,
				targetTime.AddSeconds(
					maxConnectionLatencySeconds +
					chunkTime * (hlsLstSize + safeHlsLstDelta + 1))
			);

            stopwatch.Stop();
            //Console.WriteLine("[Stopwatch]: DirectoryGetFiles:" +
                              //$"{stopwatch.ElapsedMilliseconds}");

            var fileChunks = GatherRequiredChunks(
                chunkTime,
                files,
                targetTime,
                hlsLstSize,
				lastFileIndex);

            return CombineChunksIntoPlaylist(
                channel,
                channelRoot,
                chunkTime,
                fileChunks);         
		}

		private void BasicInitializations(
			FFMPEGConfig ffmpegCfg,
            IEnumerable<Channel> streamsCfgs,
            string channel,
			out Channel streamCfg,
			out string channelRoot)
		{
			streamCfg = streamsCfgs.FirstOrDefault(x => x.Name == channel);
            if (streamCfg == null)
                throw new NoSuchChannelException(streamCfg.Name);

            channelRoot = $"{ffmpegCfg.ChunkStorageDir}/{channel}/";
		}

		private IEnumerable<ChunkFile> GatherRequiredChunks(
			double chunkTime,
			IEnumerable<string> files,
            DateTime targetTime,
            int hlsLstSize,
            int lastFileIndex = -1)
        {
			ChunkFile[] chunks;

			var targetTimeS = targetTime.Add(-DateTimeOffset.Now.Offset)
                                        .ToUnixTimeSeconds();
            
			var fileChunks = files.Select(x => new ChunkFile(x))
			                      .OrderBy(x => x.index)
			                      .ToArray();
			
			if (lastFileIndex == -1)
			{
				chunks = fileChunks
					.Where(x => x.timeSeconds >= targetTimeS - chunkTime)
                    .OrderBy(x => x.timeSeconds).ThenBy(x => x.index)
                    .Take(hlsLstSize + safeHlsLstDelta)
                    .ToArray();
			}
			else
			{
				chunks = fileChunks
					.Where(x => x.index > lastFileIndex)
                    .OrderBy(x => x.timeSeconds).ThenBy(x => x.index)
                    .Take(hlsLstSize + safeHlsLstDelta)
                    .ToArray();
			}

			var stopwatch = new Stopwatch();
            stopwatch.Start();
    //        Console.WriteLine(
				//$"[Stopwatch]: Select Chunks: {stopwatch.ElapsedMilliseconds}");

            if (chunks.Length != hlsLstSize + safeHlsLstDelta)
            {
                throw new NoAvailableFilesException(
                    chunks.Length,
                    hlsLstSize + safeHlsLstDelta,
                    $"(+{safeHlsLstDelta})");
            }

			chunks = GetConsecutiveChunks(chunks).Take(hlsLstSize)
			                                     .ToArray();

			if (chunks.Length != hlsLstSize)
            {
                throw new NoAvailableFilesException(
					chunks.Length,
                    hlsLstSize);
            }

			return chunks;
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var mostRecent =
                GetFilesInsideTimeRange(
                    chunksRoot,
					targetTime.AddSeconds(
						-maxConnectionLatencySeconds - 1.2 * chunkTime),
                    DateTime.Now
                ).OrderByDescending(File.GetLastWriteTime)
                 .FirstOrDefault();

            if (mostRecent == null)
                throw new NoAvailableFilesException(0, chunksCount);

            stopwatch.Stop();
    //        Console.WriteLine(
				//$"[Stopwatch]: GetNewest: {stopwatch.ElapsedMilliseconds}");

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

		private List<string> GetFilesInsideTimeRange(
            string root,
            DateTime minTime,
            DateTime maxTime)
        {
            var result = new List<string>();
            for (var time = minTime; time <= maxTime; time = time.AddMinutes(1))
            {
                var dir = root + GetDirOfDateTime(time);
                if (Directory.Exists(dir))
                    result.AddRange(Directory.GetFiles(dir));
            }

            return result;
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

		private M3U8Playlist CombineChunksIntoPlaylist(
			string channel,
			string channelRoot,
			double chunkTime,
			IEnumerable<ChunkFile> chunks)
		{
			return new M3U8Playlist
			{
				headers = new[]
				{
					"#EXTM3U",
					"#EXT-X-VERSION:3",
					$"#EXT-X-TARGETDURATION:{chunkTime}",
					$"#EXT-X-MEDIA-SEQUENCE:{chunks.First().index}",
				},
				files = chunks.Select(file => new M3U8File
				{
					extinf = chunkTime,
					isDiscont = StreamingProcManager
						.instance
						.chunkDiscontinuities[channel]
						.Contains(file.index),
					fileIndex = file.index,
					filePath = file.fullPath.Replace(channelRoot, "")
				})
			};
		}
	}
}
