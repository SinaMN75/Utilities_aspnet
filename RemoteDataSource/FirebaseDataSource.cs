namespace Utilities_aspnet.RemoteDataSource;

public class FirebaseFcmNotificationCreateDto {
	public required string ServerKey { get; set; }
	public required string FcmToken { get; set; }
	public required string Title { get; set; }
	public required string Body { get; set; }
	public string? Url { get; set; }
	public string? DeepLink { get; set; }
	public IEnumerable<string>? TargetUserIds { get; set; }
}

public static class FirebaseDataSource {
	public static async Task<ZibalRequestReadDto?> SendFCMNotification(FirebaseFcmNotificationCreateDto dto) {
		RestRequest request = new("https://fcm.googleapis.com/fcm/send", Method.POST);
		var body = new {
			to = dto.FcmToken,
			notification = new {
				title = dto.Title,
				body = dto.Body,
				mutable_content = true,
				sound = "Tri-tone"
			},
			data = new {
				url = dto.Url,
				dl = dto.DeepLink,
				targetUserIds = dto.TargetUserIds
			}
		};
		request.AddHeader("Content-Type", "application/json");
		request.AddHeader("Authorization", dto.ServerKey);

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(request);
		ZibalRequestReadDto? zibalRequestReadDto = ZibalRequestReadDto.FromJson(responseRequest.Content);
		return zibalRequestReadDto;
	}
}