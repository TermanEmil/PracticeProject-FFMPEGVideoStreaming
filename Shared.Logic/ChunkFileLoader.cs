using System;
using System.IO;
using System.Text.RegularExpressions;
using DataLayer;

namespace Shared.Logic
{
	public static class ChunkFileLoader
    {
		public static ChunkFile Load(string fullPath)
        {
            var fileName = Path.GetFileName(fullPath);
            var numbersStr = Regex.Split(fileName, @"\D+");         
			return new ChunkFile
			{
				fullPath = fullPath,
				timeSeconds = int.Parse(numbersStr[0]),
				index = int.Parse(numbersStr[1]),
                procID = int.Parse(numbersStr[2]),
			};
        }
    }
}
