using System;

namespace VideoStreamer.BusinessLogic.ChunksCollectors
{
	public class ChunkCollectorException : Exception
	{
		public ChunkCollectorException(string msg) : base(msg) { }
	}

	public class NoSuchChannelException : ChunkCollectorException
	{
		public NoSuchChannelException() : base("No such channel") {}

		public NoSuchChannelException(string channel)
			: base($"{channel}: No such channel")
		{
		}
	}

	public class NoAvailableFilesException : ChunkCollectorException
	{
		public NoAvailableFilesException() : base("No available files") {}

		public NoAvailableFilesException(string msg)
			: base($"No available files: {msg}")
		{
		}
	}
}
