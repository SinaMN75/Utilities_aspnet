using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Utilities
{

    public class TestCallHttpWrapper
    {
        public static async Task testMethod()
        {
            var client = new CustomHttpClient();

            var url = "https://example.com/api/resource";
            var body = "request-body"; //json Serilize
            var method = HttpMethod.Post;

            var response = await client.SendRequest(url, body, method);
        }
    }
}
