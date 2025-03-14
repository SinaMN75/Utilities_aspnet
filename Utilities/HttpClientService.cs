using Microsoft.Extensions.Caching.Memory;
namespace Utilities_aspnet.Utilities;

public interface IHttpClientService {
	Task<string> Get(string uri, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null);
	Task<string> Post(string uri,object? body,Dictionary<string, string>? headers = null,TimeSpan? cacheDuration = null);
	Task<string> Put(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<string> Delete(string uri, Dictionary<string, string>? headers = null);
}

public class HttpClientService(HttpClient httpClient, IMemoryCache cache) : IHttpClientService {
	private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
	private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

	public async Task<string> Get(
		string uri,
		Dictionary<string, string>? headers = null,
		TimeSpan? cacheDuration = null
	) {
		return await Send(HttpMethod.Get, uri, null, cacheDuration, headers);
	}

	public async Task<string> Post(
		string uri,
		object? body,
		Dictionary<string, string>? headers = null,
		TimeSpan? cacheDuration = null
	) {
		return await Send(HttpMethod.Post, uri, body, cacheDuration, headers);
	}

	public async Task<string> Put(
		string uri,
		object? body,
		Dictionary<string, string>? headers = null
	) {
		return await Send(HttpMethod.Put, uri, body, null, headers);
	}

	public async Task<string> Delete(
		string uri,
		Dictionary<string, string>? headers = null
	) {
		return await Send(HttpMethod.Delete, uri, null, null, headers);
	}

	private async Task<string> Send(
		HttpMethod method,
		string uri,
		object? body = null,
		TimeSpan? cacheDuration = null,
		Dictionary<string, string>? headers = null) {
		if (string.IsNullOrEmpty(uri)) throw new ArgumentException("URI cannot be null or empty.", nameof(uri));

		string? cacheKey = cacheDuration.HasValue ? $"{method}-{uri}" : null;
		if (cacheKey != null && _cache.TryGetValue(cacheKey, out string? cachedResponse) && cachedResponse != null)
			return cachedResponse;

		using HttpRequestMessage request = new(method, uri);
		if (body != null) {
			string json = JsonSerializer.Serialize(body);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
		}

		if (headers != null)
			foreach (KeyValuePair<string, string> header in headers)
				request.Headers.Add(header.Key, header.Value);


		using HttpResponseMessage response = await _httpClient.SendAsync(request);
		if (!response.IsSuccessStatusCode) {
			throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
		}

		string responseContent = await response.Content.ReadAsStringAsync();
		if (!cacheDuration.HasValue || cacheKey == null) return responseContent;

		MemoryCacheEntryOptions cacheOptions = new() {
			AbsoluteExpirationRelativeToNow = cacheDuration.Value
		};
		_cache.Set(cacheKey, responseContent, cacheOptions);

		return responseContent;
	}
}