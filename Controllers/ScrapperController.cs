using Utilities_aspnet.Entities.Instagram;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapperController : ControllerBase {
	
	[HttpGet("GetInstaPostRapidApi/{username}")]
	public string GetInstaPostRapidApi(string username) {
		RestClient client = new($"https://193.36.85.225/api/Scrapper/GetInstaPostRapidApi/{username}");
		client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
		RestRequest request = new(Method.GET);
		IRestResponse response = client.Execute(request);
		Console.WriteLine(response.Content);
		Console.WriteLine(response.StatusCode);
		Console.WriteLine(response.StatusCode.ToString());
		return response.Content;
	}
	
	// [HttpGet("GetInstaPostRapidApi/{username}")]
	// public async Task<InstagramUserPostDto> GetInstaPostRapidApi(string username) {
	// 	InstagramUserId userInfo = await GetUserIdFromUserName(username);
	// 	RestClient client = new($"https://instagram-scraper-20231.p.rapidapi.com/userposts/{userInfo.Data}/50/%7Bend_cursor%7D");
	// 	RestRequest request = new(Method.GET);
	// 	// request.AddHeader("X-RapidAPI-Key", "4a662212fcmshde9feeba3060587p10036ajsn69df0dd047df");
	// 	request.AddHeader("X-RapidAPI-Key", "8f4f179ebdmsh8bed1c00980c329p1de1b7jsne0fc17eb5e0c");
	// 	request.AddHeader("X-RapidAPI-Host", "instagram-scraper-20231.p.rapidapi.com");
	// 	IRestResponse response = await client.ExecuteAsync(request);
	// 	InstagramUserPost? instagramUserPost = InstagramUserPost.FromJson(response.Content);
	// 	List<InstagramPost> posts = new();
	//
	// 	foreach (DataEdge dataEdge in instagramUserPost?.Data?.Edges ?? new List<DataEdge>()) {
	// 		InstagramPost post = new() {
	// 			Desctiption = dataEdge.Node?.EdgeMediaToCaption.Edges.FirstOrDefault()?.Node.Text ?? "",
	// 			Images = dataEdge?.Node?.EdgeSidecarToChildren?.Edges?.Where(x => x.Node.IsVideo == false).Select(x => x.Node?.DisplayUrl ?? "").ToList(),
	// 			Videos = dataEdge?.Node?.EdgeSidecarToChildren?.Edges?.Where(x => x.Node.IsVideo == true).Select(x => x.Node?.DisplayUrl ?? "").ToList()
	// 		};
	// 		if (dataEdge?.Node?.IsVideo ?? false) post.Videos?.Insert(0, dataEdge.Node?.DisplayUrl ?? "");
	// 		else post.Videos?.Insert(0, dataEdge?.Node?.DisplayUrl ?? "");
	// 		posts.Add(post);
	// 	}
	// 	InstagramUserPostDto dto = new() { InstagramPosts = posts };
	//
	// 	return dto;
	// }
	//
	// private async Task<InstagramUserId> GetUserIdFromUserName(string username) {
	// 	RestClient client = new($"https://instagram-scraper-20231.p.rapidapi.com/userid/{username}");
	// 	RestRequest request = new(Method.GET);
	// 	request.AddHeader("X-RapidAPI-Key", "4a662212fcmshde9feeba3060587p10036ajsn69df0dd047df");
	// 	request.AddHeader("X-RapidAPI-Host", "instagram-scraper-20231.p.rapidapi.com");
	// 	IRestResponse response = await client.ExecuteAsync(request);
	// 	InstagramUserId instagramUserInfo = InstagramUserId.FromJson(response.Content)!;
	// 	return instagramUserInfo;
	// }
}