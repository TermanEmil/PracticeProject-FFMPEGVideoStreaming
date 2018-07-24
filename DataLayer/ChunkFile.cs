using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DataLayer
{
	public class ChunkFile
    {
        public string fullPath;
        public int timeSeconds;
        public int index;
		public int procID;
		public bool isDiscont = false;
    }
}
