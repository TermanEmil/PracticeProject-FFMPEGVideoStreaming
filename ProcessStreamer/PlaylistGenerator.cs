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
		public string fullName;
		public int totalSeconds;
		public int millisecondsDuration;

		public ChunkFile(string fullName)
		{
			this.fullName = fullName;
			var secondsStr = Regex.Match(fullName, @"\d+").Value;
			this.totalSeconds = Int32.Parse(secondsStr);

			//var millStr = Regex.Match(fullName.Skip(secondsStr.Length + 1), @"\d+")
			                   //.Value;
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
			chanel = chanel.ToLower();
			var streamCfg = streamsCfgs.FirstOrDefault(x => x.Name == chanel);

			if (streamCfg == null)
				throw new Exception("Not such chanel");

			var chanelRoot = string.Format(
                "./{0}/{1}/",
				ffmpegCfg.ChunkStorageDir,
                chanel.ToLower());
   
            var content = String.Join("\n", new[]
            {
                "#EXTM3U",
                "#EXT-X-VERSION:3",
                "#EXT-X-TARGETDURATION:6",
                "#EXT-X-MEDIA-SEQUENCE:0",
            });

			//var lastModifTime = Directory.GetLastWriteTime(chanelRoot);
			//var newestDir =
			//$"{lastModifTime.Year}/" +
			//$"{lastModifTime.Month}/" +
			//$"{lastModifTime.Day}/" +
			//$"{lastModifTime.Hour}/" +
			//$"{lastModifTime.Minute}";

			var minTime = time.AddSeconds(-streamCfg.ChunkTime);
			var maxTime = time.AddSeconds(streamCfg.ChunkTime * (hlsLstSize + 1));

			var possibleFiles = new List<string>();

			for (
				var targetTime = minTime;
				targetTime <= maxTime;
				targetTime = targetTime.AddSeconds(streamCfg.ChunkTime))
            {
                var dir =
					$"{targetTime.Year}/" +
					$"{targetTime.Month}/" +
					$"{targetTime.Day}/" +
					$"{targetTime.Hour}/" +
					$"{targetTime.Minute}";

				if (!Directory.Exists(chanelRoot + dir))
					continue;

				possibleFiles.AddRange(Directory.GetFiles(dir));
            }

            

			//possibleFiles = possibleFiles.OrderByDescending(x => )

			return content;
		}
    }
}
