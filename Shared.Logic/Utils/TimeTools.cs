using System;

namespace Shared.Logic.Utils
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

		public static DateTime SecsToDateWithOffset(int seconds)
        {
            return unixEpoch.AddSeconds(seconds)
				            .Add(DateTimeOffset.Now.Offset);
        }

		public static double CurrentSeconds()
		{
			return DateTime.Now.Add(-DateTimeOffset.Now.Offset)
				           .ToUnixTimeSeconds();
		}

		public static string GetDir(this DateTime time)
        {
            return
                $"{time.Year}/" +
                $"{time.Month.ToString("00")}/" +
                $"{time.Day.ToString("00")}/" +
                $"{time.Hour.ToString("00")}/" +
                $"{time.Minute.ToString("00")}/";
        }
    }
}
