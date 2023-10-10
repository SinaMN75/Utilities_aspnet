namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapperController : ControllerBase {
	
	[HttpGet("GetInstaPostRapidApi/{username}")]
	public string GetInstaPostRapidApi(string username) {
		RestClient client = new($"https://amnbekhar.com/api/Scrapper/GetInstaPostRapidApi/{username}") {
			RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
		};
		RestRequest request = new(Method.GET);
		IRestResponse response = client.Execute(request);
		return response.Content;
	}
}