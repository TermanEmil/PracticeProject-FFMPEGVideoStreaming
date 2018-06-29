using System;
namespace ProcessStreamer
{
	public static class HelperExtensions
    {
		private readonly static DateTime unixEpoch =
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static int ToUnixTimeSeconds(this DateTime time)
		{
			return (int)time.Subtract(unixEpoch).TotalSeconds;
		}
    }
}
