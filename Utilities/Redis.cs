namespace Utilities_aspnet.Utilities;

public static class Redis {
	public static async Task SetStringData(this IDistributedCache cache,
		string key,
		string value,
		TimeSpan? absoluteExpireTime = null,
		TimeSpan? slidingExpireTime = null) {
		DistributedCacheEntryOptions options = new() {
			AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
			SlidingExpiration = slidingExpireTime
		};

		await cache.SetStringAsync(key, value, options);
	}
	
	public static async Task<string?> GetStringData(this IDistributedCache cache, string recordId) {
		return await cache.GetStringAsync(recordId);
	}
}