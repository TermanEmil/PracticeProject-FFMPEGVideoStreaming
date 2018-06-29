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
            
			//var minTime = time.AddSeconds(-streamCfg.ChunkTime * (hlsLstSize + 1));
			//var maxTime = time.AddSeconds(streamCfg.ChunkTime);

			var possibleFiles = new List<ChunkFile>();
			var checkedDirs = new HashSet<string>();
			var timeSecs = time.ToUnixTimeSeconds();
			while (possibleFiles.Count() < hlsLstSize)
			{
				var dir =
                    chanelRoot +
					$"{time.Year}/" +
					$"{time.Month.ToString("00")}/" +
					$"{time.Day.ToString("00")}/" +
					$"{time.Hour.ToString("00")}/" +
					$"{time.Minute.ToString("00")}/";

				if (checkedDirs.Contains(dir) || !Directory.Exists(dir))
					break;

				var files = Directory.GetFiles(dir)
									 .Select(x => new ChunkFile(x));
				                     //.Where(x => x.timeSeconds <= timeSecs);

				possibleFiles.AddRange(files);            
				checkedDirs.Add(dir);

				var oldestFileSeconds = files.Max(x => x.timeSeconds);
				time = TimeExtensions.unixEpoch
				                     .AddSeconds(oldestFileSeconds)
				                     .AddSeconds(-streamCfg.ChunkTime);
			}

			if (!possibleFiles.Any())
				throw new Exception("No available files");
            
			possibleFiles = possibleFiles.Where(x =>
			                                    x.millsDuration > 0)
			                             .OrderByDescending(x => x.timeSeconds)
			                             .Take(hlsLstSize)
			                             .OrderBy(x => x.timeSeconds)
			                             .ToList();
			
			var content = String.Join("\n", new[]
            {
                "#EXTM3U",
                "#EXT-X-VERSION:3",
				$"#EXT-X-TARGETDURATION:{streamCfg.ChunkTime}",
				$"#EXT-X-MEDIA-SEQUENCE:{possibleFiles[0].index}\n",
            });

			ChunkFile lastFile = null;
			foreach (var file in possibleFiles)
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
    }
}
