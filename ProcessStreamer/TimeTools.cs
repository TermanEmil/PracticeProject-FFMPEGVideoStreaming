using System;

namespace ProcessStreamer
{
	public static class TimeTools
    {
		public readonly static DateTime unixEpoch =
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static int ToUnixTimeSeconds(this DateTime time)
		{
			return (int)time.Subtract(unixEpoch).TotalSeconds;
		}

		public static DateTime SecondsToDateTime(int seconds)
		{
			return unixEpoch.AddSeconds(seconds);
		}
    }
}
