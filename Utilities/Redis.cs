namespace Utilities_aspnet.Utilities;

public static class Redis {
	public static void SetStringData(
		this IDistributedCache cache,
		string key,
		string value,
		TimeSpan? absoluteExpireTime = null,
		TimeSpan? slidingExpireTime = null) {
		DistributedCacheEntryOptions options = new() {
			AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
			SlidingExpiration = slidingExpireTime
		};

		cache.SetStringAsync(key, value, options);
	}

	public static Task<string?> GetStringData(this IDistributedCache cache, string recordId) => cache.GetStringAsync(recordId);
}