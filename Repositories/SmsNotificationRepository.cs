namespace Utilities_aspnet.Repositories;

public interface ISmsNotificationRepository {
	void SendSms(string mobileNumber, string message);
	public Task<GenericResponse> SendNotification(NotificationCreateDto dto);
}

public class SmsNotificationRepository : ISmsNotificationRepository {
	private readonly IConfiguration _config;

	public SmsNotificationRepository(IConfiguration config) => _config = config;

	public async void SendSms(string mobileNumber, string message) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		SmsPanelSettings smsSetting = appSettings.SmsPanelSettings;

		switch (smsSetting.Provider) {
			case "ghasedak": {
				RestRequest request = new(Method.POST);
				request.AddHeader("apikey", smsSetting.SmsApiKey!);
				request.AddParameter("receptor", mobileNumber);
				request.AddParameter("type", 1);
				request.AddParameter("template", smsSetting.PatternCode!);
				request.AddParameter("param1", message);
				await new RestClient("https://api.ghasedak.me/v2/verification/send/simple").ExecuteAsync(request);
				break;
			}
			case "faraz": {
				RestRequest request = new(Method.POST);
				request.AddJsonBody("{\"op\" : \"pattern\"" +
				                    ",\"user\" : \"" + smsSetting.UserName + "\"" +
				                    ",\"pass\":  \"" + smsSetting.SmsSecret + "\"" +
				                    ",\"fromNum\" : \"03000505\"" +
				                    ",\"toNum\": \"" + mobileNumber + "\"" +
				                    ",\"patternCode\": \"" + smsSetting.PatternCode + "\"" +
				                    ",\"inputData\" : [{\"verification-code\": \"" + message + "\"}]}");
				await new RestClient("http://ippanel.com/api/select").ExecuteAsync(request);
				break;
			}
		}
	}

	public async Task<GenericResponse> SendNotification(NotificationCreateDto dto) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		PushNotificationSetting setting = appSettings.PushNotificationSetting;

		switch (setting.Provider) {
			case "pushe": {
				RestRequest request = new(Method.POST);
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("Authorization", "Token " + setting.Token);
				var body = new {
					app_ids = setting.AppId,
					data = new {
						title = dto.Title,
						content = dto.Content,
						bigContent = dto.BigContent,
						action = new { action_type = dto.ActionType, url = dto.Url }
					},
					is_draft = false,
					filter = new { custom_id = dto.UserIds }
				};
				request.AddJsonBody(body);
				await new RestClient("https://api.pushe.co/v2/messaging/notifications/").ExecuteAsync(request);
				break;
			}
		}
		return new GenericResponse();
	}
}

public class NotificationCreateDto {
	public IEnumerable<string> UserIds { get; set; } = new List<string>();
	public string Title { get; set; } = "";
	public string Content { get; set; } = "";
	public string BigContent { get; set; } = "";
	public string Url { get; set; } = "";
	public string ActionType { get; set; } = "U";
}