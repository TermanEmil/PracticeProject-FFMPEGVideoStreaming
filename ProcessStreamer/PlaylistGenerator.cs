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

		public ChunkFile(string fullPath)
		{
			this.fullPath = fullPath;
			var fileName = Path.GetFileName(fullPath);
			var secondsStr = Regex.Match(fileName, @"\d+").Value;
			this.timeSeconds = Int32.Parse(secondsStr);

			var millStr = Regex.Match(fileName.Substring(secondsStr.Length), @"\d+")
			                   .Value;
			this.millsDuration = Int32.Parse(millStr);
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
            
			var minTime = time.AddSeconds(-streamCfg.ChunkTime * (hlsLstSize + 1));
			var maxTime = time.AddSeconds(streamCfg.ChunkTime);

			var possibleFiles = new List<ChunkFile>();
			var checkedDirs = new HashSet<string>();

			for (
				var targetTime = maxTime;
				targetTime >= minTime;
				targetTime = targetTime.AddSeconds(-streamCfg.ChunkTime))
            {
                var dir =
					chanelRoot +
					$"{targetTime.Year}/" +
					$"{targetTime.Month.ToString("00")}/" +
					$"{targetTime.Day.ToString("00")}/" +
					$"{targetTime.Hour.ToString("00")}/" +
					$"{targetTime.Minute.ToString("00")}/";

				if (checkedDirs.Contains(dir) || !Directory.Exists(dir))
					continue;
                
				possibleFiles.AddRange(
					Directory.GetFiles(dir).Select(x => new ChunkFile(x)));
				checkedDirs.Add(dir);
            }

			if (!possibleFiles.Any())
				throw new Exception("No available files");

			var timeSecs = time.ToUnixTimeSeconds();

			possibleFiles = possibleFiles
				.Where(x =>
			                                    ////x.timeSeconds - streamCfg.ChunkTime < timeSecs &&
			                                    x.millsDuration > 0)
			                             .OrderBy(x => x.timeSeconds)
			                             .Take(hlsLstSize)
			                             .ToList();

			var content = String.Join("\n", new[]
            {
                "#EXTM3U",
                "#EXT-X-VERSION:3",
				$"#EXT-X-TARGETDURATION:{streamCfg.ChunkTime}",
                "#EXT-X-MEDIA-SEQUENCE:0\n",
            });

			foreach (var file in possibleFiles)
			{
				content += $"#EXTINF:{file.GetMillisecondsStr()}," + "\n";
				content += file.fullPath.Replace(chanelRoot, "") + "\n";
			}

			return content;
		}
    }
}
