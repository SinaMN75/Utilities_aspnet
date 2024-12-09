using Microsoft.Extensions.Caching.Memory;

namespace Utilities_aspnet.Repositories;

public interface ICacheRepository {
	string? GetString(string key);
	void SetString(string key, string value, TimeSpan? expireDate = null);
	void Delete(string key);
}

public class RedisCacheRepository(IDistributedCache cache) : ICacheRepository {
	public string? GetString(string key) => cache.GetString(key);

	public void Delete(string key) => cache.Remove(key);

	public void SetString(string key, string value, TimeSpan? expireDate) =>
		cache.SetString(key, value, new DistributedCacheEntryOptions {
			AbsoluteExpirationRelativeToNow = expireDate ?? TimeSpan.FromSeconds(60),
			SlidingExpiration = expireDate
		});
}

public class MemoryCacheRepository(IMemoryCache cache) : ICacheRepository {
	public string? GetString(string key) => cache.Get<string>(key);

	public void Delete(string key) => cache.Remove(key);

	public void SetString(string key, string value, TimeSpan? expireDate) =>
		cache.Set(key, value, expireDate ?? TimeSpan.FromSeconds(60));
}