using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VideoStreamer.DB;

namespace VideoStreamer.Utils
{
	public static class StreamerSessionCleaner
    {
		public static double cleanupIntervalSeconds = 1 * 60;
		private static DateTime lastCleanup = default(DateTime);

		public static async Task TryCleanupExpiredSession(
			IServiceProvider serviceProvider)
		{
			if (DateTime.Now > lastCleanup.AddSeconds(cleanupIntervalSeconds))
			{
				lastCleanup = DateTime.Now;            
				await Task.Run(() => DoCleanup(serviceProvider));
			}
		}

		private static void DoCleanup(IServiceProvider serviceProvider)
		{
			var db = serviceProvider.GetService<StreamerContext>();
			var expiredSessions =
				db.StreamingSessions.Where(x => DateTime.Now > x.ExpireTime)
				  .ToArray();

			if (expiredSessions.Length > 0)
			{
				db.StreamingSessions.RemoveRange(expiredSessions);
				db.SaveChanges();
			}
		}
    }
}
