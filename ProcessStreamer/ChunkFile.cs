using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ProcessStreamer
{
	public class ChunkFile
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
}
