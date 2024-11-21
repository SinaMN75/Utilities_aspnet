namespace Utilities_aspnet.Utilities;

public class HttpClientInterceptor(HttpClient httpClient) {
	public async Task<TResponse?> SendAsync<TRequest, TResponse>(
		string url,
		HttpMethod httpMethod,
		TRequest requestBody,
		Dictionary<string, string>? headers = null) {
		try {
			HttpRequestMessage requestMessage = new(httpMethod, url);

			if (requestBody != null)
				requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

			if (headers != null)
				foreach (KeyValuePair<string, string> header in headers)
					requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

			HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
			string responseBody = await response.Content.ReadAsStringAsync();

			response.EnsureSuccessStatusCode();

			return JsonConvert.DeserializeObject<TResponse>(responseBody);
		}
		catch (Exception ex) {
			Console.WriteLine($"Exception occurred: {ex.Message}");
			throw;
		}
	}
}