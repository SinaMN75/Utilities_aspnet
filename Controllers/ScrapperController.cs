using Utilities_aspnet.Entities.Instagram;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapperController : ControllerBase {
	[HttpGet("ScrapInstaPostByUserName/{username}")]
	public async Task<string> ScrapInstaPostByUserName(string username) {
		RestRequest request = new(Method.POST);
		request.AddParameter("accusername", "saba.mirzaei22");
		request.AddParameter("accpassword", "Aa!@#123");
		request.AddParameter("targetusername", username);
		request.AddParameter("postnumbers", 50);
		IRestResponse response = await new RestClient("http://193.36.85.225:2096/api/getdatainstagram").ExecuteAsync(request);
		return response.Content;
	}

	[HttpGet("GetInstaPostRapidApi/{username}")]
	public async Task<InstagramUserPostDto> GetInstaPostRapidApi(string username) {
		InstagramUserId userInfo = await GetUserIdFromUserName(username);
		RestClient client = new($"https://instagram-scraper-20231.p.rapidapi.com/userposts/{userInfo.Data}/50/%7Bend_cursor%7D");
		RestRequest request = new(Method.GET);
		request.AddHeader("X-RapidAPI-Key", "4a662212fcmshde9feeba3060587p10036ajsn69df0dd047df");
		request.AddHeader("X-RapidAPI-Host", "instagram-scraper-20231.p.rapidapi.com");
		IRestResponse response = await client.ExecuteAsync(request);
		InstagramUserPost? instagramUserPost = InstagramUserPost.FromJson(response.Content);
		List<InstagramPost> posts = new();

		try {
			foreach (DataEdge dataEdge in instagramUserPost?.Data?.Edges ?? new List<DataEdge>()) {
				posts.Add(
					new InstagramPost {
						Desctiption = dataEdge.Node.EdgeMediaToCaption.Edges.FirstOrDefault()?.Node.Text ?? "",
						Images = dataEdge.Node.EdgeSidecarToChildren.Edges.Where(x => x.Node.IsVideo == false).Select(x => x.Node.DisplayUrl.AbsoluteUri).ToList(),
						Videos = dataEdge.Node.EdgeSidecarToChildren.Edges.Where(x => x.Node.IsVideo == true).Select(x => x.Node.DisplayUrl.AbsoluteUri).ToList()
					});
			}
		}
		catch (Exception e) {
			Console.WriteLine(e.StackTrace);
		}
		InstagramUserPostDto dto = new() { InstagramPosts = posts };


		return dto;
	}

	private async Task<InstagramUserId> GetUserIdFromUserName(string username) {
		RestClient client = new($"https://instagram-scraper-20231.p.rapidapi.com/userid/{username}");
		RestRequest request = new(Method.GET);
		request.AddHeader("X-RapidAPI-Key", "4a662212fcmshde9feeba3060587p10036ajsn69df0dd047df");
		request.AddHeader("X-RapidAPI-Host", "instagram-scraper-20231.p.rapidapi.com");
		IRestResponse response = await client.ExecuteAsync(request);
		InstagramUserId instagramUserInfo = InstagramUserId.FromJson(response.Content)!;
		return instagramUserInfo;
	}
}