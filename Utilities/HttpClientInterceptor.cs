namespace Utilities_aspnet.Utilities;

public class HttpClientInterceptor {
	private readonly HttpClient _httpClient = new();

	public void SetBaseAddress(string baseAddress) {
		_httpClient.BaseAddress = new Uri(baseAddress);
	}

	public void AddHeader(string name, string value) {
		_httpClient.DefaultRequestHeaders.Add(name, value);
	}

	public async Task<T?> GetAsync<T>(string endpoint) {
		HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
		response.EnsureSuccessStatusCode();

		string json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, Core.JsonSettings);
	}

	public async Task<string> GetRawAsync(string endpoint) {
		HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	public async Task<string> PostAsync(string endpoint, object? data = null) {
		HttpResponseMessage response =
			await _httpClient.PostAsync(endpoint, data == null ? null : new StringContent(JsonSerializer.Serialize(data, Core.JsonSettings), Encoding.UTF8, "application/json"));
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	public async Task<T?> PutAsync<T, TU>(string endpoint, TU data) {
		StringContent content = new(JsonSerializer.Serialize(data, Core.JsonSettings), Encoding.UTF8, "application/json");
		HttpResponseMessage response = await _httpClient.PutAsync(endpoint, content);
		response.EnsureSuccessStatusCode();

		string json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, Core.JsonSettings);
	}

	public async Task DeleteAsync(string endpoint) {
		HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);
		response.EnsureSuccessStatusCode();
	}

	public void Dispose() {
		_httpClient.Dispose();
	}
}