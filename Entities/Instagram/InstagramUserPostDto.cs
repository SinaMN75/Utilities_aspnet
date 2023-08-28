namespace Utilities_aspnet.Entities.Instagram; 

public class InstagramUserPostDto {
	public required List<InstagramPost> InstagramPosts { get; set; }
}

public class InstagramPost {
	public string Desctiption { get; set; } = "";
	public List<string> Images { get; set; } = new();
	public List<string> Videos { get; set; } = new();
}