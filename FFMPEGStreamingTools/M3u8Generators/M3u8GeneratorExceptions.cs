using System;
using System.Runtime.Serialization;

namespace FFMPEGStreamingTools.M3u8Generators
{
	public class M3U8GeneratorException : Exception
	{
		public M3U8GeneratorException(string msg) : base(msg)
		{
		}
	}

	public class NoSuchChannelException : M3U8GeneratorException
	{
		public NoSuchChannelException(string channel)
			: base($"{channel}: No such channel")
		{
		}
	}

	public class NoAvailableFilesException : M3U8GeneratorException
	{
		public NoAvailableFilesException(
			int currentCount,
			int targetCount,
			string extraMsg = ""
		) : base($"No Available files: {currentCount}/{targetCount} {extraMsg}")
		{
		}
	}
}
