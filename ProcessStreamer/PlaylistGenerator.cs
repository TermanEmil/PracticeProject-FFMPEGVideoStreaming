using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace ProcessStreamer
{
	class ChunkFile
	{
		public string fullPath;
		public int timeSeconds;
		public int millsDuration;
		public int index;

		public ChunkFile(string fullPath)
		{
			this.fullPath = fullPath;
			var fileName = Path.GetFileName(fullPath);

			var numbersStr = Regex.Split(fileName, @"\D+");
			this.timeSeconds = int.Parse(numbersStr[0]);
			this.millsDuration = int.Parse(numbersStr[1]);
			this.index = int.Parse(numbersStr[2]);
		}

		public string GetMillisecondsStr()
		{
			var duration = ((int)(millsDuration / 1000000)).ToString();
			var millsStr = millsDuration.ToString();

			return duration + "." + millsStr.Substring(duration.Length);
		}
	}

    public class PlaylistGenerator
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
				throw new Exception("Not such chanel");

			var chanelRoot = string.Format(
                "./{0}/{1}/",
				ffmpegCfg.ChunkStorageDir,
                chanel);

			time = time.AddSeconds(-streamCfg.ChunkTime * 1.2f);
			var timeSecs = time.Add(-DateTimeOffset.Now.Offset).ToUnixTimeSeconds();

			var dir = chanelRoot + GetDirOfDateTime(time);

			if (!Directory.Exists(dir))
				throw new Exception("No available files");

			var files = Directory.GetFiles(dir)
			                     .Select(x => new ChunkFile(x))
			                     .ToArray();
			var filtered = files.Where(
				x => x.millsDuration > 0 &&
				x.timeSeconds <= timeSecs &&
				Math.Abs(x.timeSeconds - timeSecs) < streamCfg.ChunkTime * 1.2
			).ToArray();
            
			if (filtered.Length == 0)
				throw new Exception("No available files");

			var closest = filtered.OrderBy(x => Math.Abs(x.timeSeconds - timeSecs))
			                      .First();
			
			var refTime = TimeTools.SecondsToDateTime(closest.timeSeconds);
			var chunks = new List<ChunkFile>(hlsLstSize);
			var nextIndex = closest.index;

			while (chunks.Count() < hlsLstSize)
			{
				var chunksDir = chanelRoot + GetDirOfDateTime(refTime.Add(DateTimeOffset.Now.Offset));

				if (!Directory.Exists(chunksDir))
					break;

				var dirChunks = Directory.GetFiles(chunksDir)
				                         .Select(x => new ChunkFile(x))
				                         .OrderBy(x => x.index)
										 .ToArray();
				
				if (dirChunks.Length == 0)
				{
					break;
				}
				
				bool endOfFiles = false;
				while (nextIndex >= 0 && chunks.Count() < hlsLstSize)
				{
					var targetFile = dirChunks.FirstOrDefault(x => x.index == nextIndex);
                    
					if (targetFile == null)
					{
						refTime = refTime.AddMinutes(-1);
						break;
					}

					if (targetFile == null || targetFile.millsDuration <= 0)
					{
						endOfFiles = true;
						break;
					}

					chunks.Add(targetFile);

					refTime = TimeTools.SecondsToDateTime(targetFile.timeSeconds);
					nextIndex = targetFile.index - 1;
				}
                
				if (nextIndex < 0 || endOfFiles)
					break;
			}         
			         
			if (!chunks.Any())
				throw new Exception("No available files");
                    
			if (chunks.Count() != hlsLstSize)
			{
				var a = 1;
				throw new Exception($"{DateTime.Now.ToUnixTimeSeconds()}Invalid nb of chunks: {chunks.Count}/{hlsLstSize}");
			}
				

			var content = String.Join("\n", new[]
            {
                "#EXTM3U",
                "#EXT-X-VERSION:3",
				$"#EXT-X-TARGETDURATION:{streamCfg.ChunkTime}",
				$"#EXT-X-MEDIA-SEQUENCE:{chunks[0].index}\n",
            });

			ChunkFile lastFile = null;
			foreach (var file in chunks.OrderBy(x => x.index))
			{
				// Break if the difference is much biggere than Chunk time.
				if (lastFile != null)
				{
					var seconcdsDiff = Math.Abs(
						file.timeSeconds - lastFile.timeSeconds);

					if (seconcdsDiff > streamCfg.ChunkTime * 1.1f)
					    break;
				}

				content += $"#EXTINF:{file.GetMillisecondsStr()}," + "\n";
				content += file.fullPath.Replace(chanelRoot, "") + "\n";

				lastFile = file;
			}

			return content;
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
