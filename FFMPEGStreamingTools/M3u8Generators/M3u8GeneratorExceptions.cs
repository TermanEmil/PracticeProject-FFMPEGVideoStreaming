using System;
using System.Runtime.Serialization;

namespace FFMPEGStreamingTools.M3u8Generators
{
	public class NoSuchChannelException : Exception
	{
		public NoSuchChannelException(string channel)
			: base($"{channel}: No such channel")
		{
		}
	}

	public class NoAvailableFilesException : Exception
	{
		public NoAvailableFilesException(
			int currentCount,
			int targetCount,
			string extraMsg = "")
			: base($"No Available files: {currentCount}/{targetCount} {extraMsg}")
		{
		}
	}
}
