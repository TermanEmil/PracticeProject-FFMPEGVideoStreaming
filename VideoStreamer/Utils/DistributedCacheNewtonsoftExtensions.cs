using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using VideoStreamer.DB;

namespace VideoStreamer.Utils
{
	public static class DistributedCacheNewtonsoftExtensions
    {
		public static async Task<T> GetAsync<T>(
            this IDistributedCache cache,
			string key) where T : class
		{
			var value = await cache.GetStringAsync(key);

			if (value == null)
				return null;
         
			return JsonConvert.DeserializeObject<T>(value);
        }
        
		public static async Task SetObjAsync(
			this IDistributedCache cache,
			string key,
			object anyObj,
			DistributedCacheEntryOptions options = null)
		{
			var json = JsonConvert.SerializeObject(anyObj);
			await cache.SetStringAsync(key, json, options);
		}
    }
}
