using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Utilities
{
    public class CustomHttpClient<TIn, TOut> : HttpClient where TIn : class where TOut : class
    {
        public CustomHttpClient() : base(new CustomInterceptor())
        {
            DefaultRequestHeaders.Accept.Clear();
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<TOut> SendRequest(TIn data, string url, HttpMethod httpMethod, HttpRequestHeaders? headers)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(httpMethod, url) { Content = content };
            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            var response = await SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TOut>(responseString);
        }       
    }


    public class CustomInterceptor : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }                        
        }
        public CustomInterceptor()
        {
            InnerHandler = new HttpClientHandler();
        }
    }
}
