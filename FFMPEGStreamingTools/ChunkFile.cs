﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FFMPEGStreamingTools
{
	public class ChunkFile
    {
        public string fullPath;
        public int timeSeconds;
        public int index;
		public int procID;
		public bool isDiscont = false;
        
        public ChunkFile(string fullPath)
        {
            this.fullPath = fullPath;
            var fileName = Path.GetFileName(fullPath);

            var numbersStr = Regex.Split(fileName, @"\D+");
            this.timeSeconds = int.Parse(numbersStr[0]);
            this.index = int.Parse(numbersStr[1]);
			this.procID = int.Parse(numbersStr[2]);
        }
    }
}
