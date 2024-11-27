namespace Utilities_aspnet.Utilities;

public static class Redis {
	public static void SetStringData(
		this IDistributedCache cache,
		string key,
		string value,
		TimeSpan? absoluteExpireTime = null,
		TimeSpan? slidingExpireTime = null
	) {
		cache.SetStringAsync(key, value, new DistributedCacheEntryOptions {
			AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
			SlidingExpiration = slidingExpireTime
		});
	}

	public static Task<string?> GetStringData(this IDistributedCache cache, string recordId) => cache.GetStringAsync(recordId);
}