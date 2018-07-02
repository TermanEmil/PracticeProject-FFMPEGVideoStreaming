using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MoreLinq;

namespace ProcessStreamer
{   
	public static class PlaylistGenerator
    {         
		public static string GeneratePlaylist(
			string chanel,
			DateTime time,
			FFMPEGConfig ffmpegCfg,
			IEnumerable<StreamConfig> streamsCfgs,
			int hlsLstSize = 5)
		{
			var streamCfg = streamsCfgs.FirstOrDefault(x => x.Name == chanel);         
			if (streamCfg == null)
				throw new Exception("No such chanel");

			var chanelRoot = $"{ffmpegCfg.ChunkStorageDir}/{chanel}/";

			var targetTime = GetMinChunkTimeSpan(
				hlsLstSize, streamCfg.ChunkTime, time, chanelRoot);

			var targetTimeS = targetTime.Add(-DateTimeOffset.Now.Offset)
                                        .ToUnixTimeSeconds();
			var chunks =
				Directory.GetFiles(chanelRoot, "*.ts", SearchOption.AllDirectories)
			        .Select(x => new ChunkFile(x))
			        .Where(x =>
				           x.timeSeconds >= targetTimeS - streamCfg.ChunkTime &&
				           x.millsDuration > 0)
			        .OrderBy(x => x.timeSeconds)
                    .Take(hlsLstSize);

			var fileChunks = GetContinuousChunks(chunks).ToArray();         
			if (fileChunks.Length < hlsLstSize)
			{
				throw new Exception(string.Format(
					"No available files: {0}/{1}",
					fileChunks.Length,
					hlsLstSize));
			}

			var content = String.Join("\n", new[]
            {
                "#EXTM3U",
                "#EXT-X-VERSION:3",
				$"#EXT-X-TARGETDURATION:{streamCfg.ChunkTime}",
                $"#EXT-X-MEDIA-SEQUENCE:{fileChunks[0].index}",
				//$"#Stopwatch:{timer1.Elapsed.TotalMilliseconds}/{timer2.Elapsed.TotalMilliseconds}",
				//$"#RequestTime:{targetTimeS}"
            });
			content += ",\n";
            
			foreach (var file in fileChunks)
            {
                content += $"#EXTINF:{file.GetMillisecondsStr()}," + "\n";
                content += file.fullPath.Replace(chanelRoot, "") + "\n";
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
		private static DateTime GetMinChunkTimeSpan(
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

		private static IEnumerable<ChunkFile> GetContinuousChunks(
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

		private static string GetDirOfDateTime(DateTime time)
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
