namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapperController : BaseApiController {
	[HttpGet("ScrapInstaPostByUserName/{username}")]
	public static async Task<ActionResult> ScrapInstaPostByUserName(string username) {
		RestRequest request = new(Method.POST);
		request.AddParameter("accusername", "dors.2023");
		request.AddParameter("accpassword", "Dors@2023");
		request.AddParameter("targetusername", username);
		request.AddParameter("postnumbers", 50);
		IRestResponse response = await new RestClient("http://193.36.85.225:2096/api/getdatainstagram").ExecuteAsync(request);
		return Result(new GenericResponse<string>(response.Content));
	}
}