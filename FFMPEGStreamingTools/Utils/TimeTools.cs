using System;

namespace FFMPEGStreamingTools.Utils
{
	public static class TimeTools
    {
		public readonly static DateTime unixEpoch =
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static double ToUnixTimeSeconds(this DateTime time)
		{
			return time.Subtract(unixEpoch).TotalSeconds;
		}

		public static DateTime SecondsToDateTime(int seconds)
		{
			return unixEpoch.AddSeconds(seconds);
		}

		public static double CurrentSeconds()
		{
			return DateTime.Now.Add(-DateTimeOffset.Now.Offset)
				           .ToUnixTimeSeconds();
		}
    }
}
