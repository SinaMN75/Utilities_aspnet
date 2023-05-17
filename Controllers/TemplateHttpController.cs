using System.Net.Http.Headers;

namespace Utilities_aspnet.Controllers
{
    [ApiController]
    [Route("api/templateHttp")]
    public class TemplateHttpController : BaseApiController
    {
        private readonly string token;
        public TemplateHttpController()
        {
            token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1MmMxMTdkOC01ZTJiLTQyZTItODA0MS0yZGY3NGE0OWVmZWIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjUyYzExN2Q4LTVlMmItNDJlMi04MDQxLTJkZjc0YTQ5ZWZlYiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiI1MmMxMTdkOC01ZTJiLTQyZTItODA0MS0yZGY3NGE0OWVmZWIiLCJJc0xvZ2dlZEluIjoiVHJ1ZSIsImp0aSI6ImE0OTU0OWJkLTI5NTAtNGEwNy1iZDRlLTM5M2ZkOTJlNmVkYiIsImV4cCI6MTcwOTA5NTEyMywiaXNzIjoiaHR0cHM6Ly9TaW5hTU43NS5jb20iLCJhdWQiOiJodHRwczovL1NpbmFNTjc1LmNvbSJ9.S-JLDDiSU8znafg226Kr6zsYMrG43dyhmFIS8x2XjiY";
        }
        [HttpPost]
        public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateUpdateDto dto)
        {
            var client = new CustomHttpClient<AddressCreateUpdateDto, GenericResponse<AddressEntity>>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await client.SendRequest(dto, "https://api.sinamn75.com/api/address", HttpMethod.Post, null);

            //var url = "https://api.sinamn75.com/api/address";
            //var method = HttpMethod.Post;
            //var response = await client.SendRequest(url, body, method);
        }

        [HttpPut]
        public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressCreateUpdateDto dto)
        {
            var client = new CustomHttpClient<AddressCreateUpdateDto, GenericResponse<AddressEntity>>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await client.SendRequest(dto, "https://api.sinamn75.com/api/address", HttpMethod.Put, null);
        }

        [HttpGet]
        public async Task<ActionResult<GenericResponse<ProductEntity>>> Get()
        {
            var client = new CustomHttpClient<object, GenericResponse<ProductEntity>>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            return await client.SendRequest(null, "https://api.sinamn75.com/api/ProductV2/14F3561A-F0EC-42B2-41C9-08DA86C505D6", HttpMethod.Get, null);
        }

        [HttpDelete]
        public async Task<ActionResult<GenericResponse>> Delete()
        {
            var client = new CustomHttpClient<object, GenericResponse>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await client.SendRequest(null, "https://localhost:7125/api/address?addressId=3fa85f64-5717-4562-b3fc-2c963f66afa6", HttpMethod.Delete, null);
        }
    }
}
