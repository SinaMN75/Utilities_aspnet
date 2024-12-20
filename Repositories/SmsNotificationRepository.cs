namespace Utilities_aspnet.Repositories;

public interface ISmsNotificationRepository {
	Task<GenericResponse> SendSms(string mobileNumber, string template, string param1, string? param2 = null, string? param3 = null);
	public Task<GenericResponse> SendNotification(NotificationCreateDto dto);
}

public class SmsNotificationRepository : ISmsNotificationRepository {
	public async Task<GenericResponse> SendSms(
		string mobileNumber,
		string template,
		string param1,
		string? param2 = null,
		string? param3 = null
	) {
		SmsPanelSettings smsSetting = AppSettings.Settings.SmsPanelSettings;

		switch (smsSetting.Provider) {
			case "ghasedak": {
				HttpClientInterceptor client = new();
				client.AddHeader("apikey", smsSetting.SmsApiKey!);
				await client.PostAsync("https://api.ghasedak.me/v2/verification/send/simple", new {
					receptor = mobileNumber,
					type = 1,
					template = smsSetting.PatternCode!,
					param1,
					param2,
					param3
				});
				break;
			}
			case "faraz": {
				break;
			}
			case "kavenegar": {
				HttpClientInterceptor client = new();
				client.AddHeader("apikey", smsSetting.SmsApiKey!);
				await client.PostAsync($"https://api.kavenegar.com/v1/{smsSetting.SmsApiKey}/verify/lookup.json", new {
					receptor = mobileNumber,
					template,
					token = param1,
					token2 = param2,
					token3 = param3,
				});
				break;
			}
		}

		return new GenericResponse();
	}

	public async Task<GenericResponse> SendNotification(NotificationCreateDto dto) {
		PushNotificationSetting setting = AppSettings.Settings.PushNotificationSetting;

		switch (setting.Provider) {
			case "pushe": {
				break;
			}
			case "firebase": {
				HttpClientInterceptor client = new();
				client.AddHeader("Content-Type", "application/json");
				client.AddHeader("Authorization", "Bearer " + setting.Token);
				await client.PostAsync("https://fcm.sinamn75.com/send", new {
					appId = setting.AppId,
					title = NotificationCreateDto.Title,
					body = NotificationCreateDto.Body,
					content = NotificationCreateDto.Content,
					token = dto.FcmToken
				});
				break;
			}
		}

		return new GenericResponse();
	}
}

public class NotificationCreateDto {
	public IEnumerable<string> UserIds { get; } = new List<string>();
	public static string Title => "";
	public static string Body => "";
	public static string Content => "";
	public static string BigContent => "";
	public static string Url => "";
	public static string ActionType => "U";
	public string? FcmToken { get; set; }
}