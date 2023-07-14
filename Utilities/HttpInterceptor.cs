namespace Utilities_aspnet.Utilities;

public class CustomHttpClient<TIn, TOut> : HttpClient where TIn : class where TOut : class
{
    public CustomHttpClient() : base(new CustomInterceptor())
    {
    }

    public async Task<TOut> SendRequest(HttpMethod httpMethod, string url, TIn body, HttpRequestHeaders? headers)
    {
        var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(httpMethod, url)
        {
            Content = content            
        };
        if (headers != null)
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
                request.Headers.Add(header.Key, header.Value);
        HttpResponseMessage response = await SendAsync(request);
        string responseString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TOut>(responseString)!;
    }

    public async Task<TOut> Post(string url, TIn body, HttpRequestHeaders? headers = null) => await SendRequest(HttpMethod.Post, url, body, headers);

    public async Task<TOut> Get(string url, HttpRequestHeaders? headers = null) => await SendRequest(HttpMethod.Get, url, null, headers);

    public async Task<TOut> Put(string url, TIn body, HttpRequestHeaders? headers = null) => await SendRequest(HttpMethod.Put, url, body, headers);

    public async Task<TOut> Delete(string url, HttpRequestHeaders? headers = null) => await SendRequest(HttpMethod.Get, url, null, headers);
}

public class CustomInterceptor : DelegatingHandler
{
    public CustomInterceptor() => InnerHandler = new HttpClientHandler();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception) { throw new Exception(); }
    }
}