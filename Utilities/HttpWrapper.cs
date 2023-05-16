using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Utilities
{
    public class CustomHttpClient : HttpClient
    {
        public CustomHttpClient() : base(new CustomInterceptor())
        {
            DefaultRequestHeaders.Add("header-name", "header-value");
        }

        public async Task<string> SendRequest(string url, string body, HttpMethod method)
        {
            var content = new StringContent(body);

            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            var response = await SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }
    }

    public class CustomInterceptor : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Modify the request here

            var response = await base.SendAsync(request, cancellationToken);

            // Modify the response here

            return response;
        }
    }
}
