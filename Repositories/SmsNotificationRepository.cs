﻿namespace Utilities_aspnet.Repositories;

public interface ISmsNotificationRepository {
	void SendSms(string mobileNumber, string message);
	public Task<GenericResponse> SendNotification(NotificationCreateDto dto);
}

public class SmsNotificationRepository : ISmsNotificationRepository {
	private readonly IConfiguration _config;

	public SmsNotificationRepository(IConfiguration config) => _config = config;

	public void SendSms(string mobileNumber, string message) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		SmsPanelSettings smsSetting = appSettings.SmsPanelSettings;

		if (mobileNumber.Contains("+98")) {
			mobileNumber = mobileNumber.TrimStart(new[] {'+'});
			mobileNumber = mobileNumber.TrimStart(new[] {'9'});
			mobileNumber = mobileNumber.TrimStart(new[] {'8'});
		}
		else mobileNumber = mobileNumber.TrimStart(new[] {'0'});

		switch (smsSetting.Provider) {
			case "ghasedak": {
				Api sms = new(smsSetting.SmsApiKey);
				sms.VerifyAsync(1, smsSetting.PatternCode, new[] {mobileNumber}, message);
				break;
			}
			case "faraz": {
				RestClient client = new("http://188.0.240.110/api/select");
				RestRequest request = new(Method.POST);
				request.AddHeader("cache-control", "no-cache");
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("Authorization", "AccessKey " + smsSetting.SmsApiKey);
				request.AddParameter("undefined",
				                     "{\"op\" : \"pattern\"" + ",\"user\" : \"" + smsSetting.UserName + "\"" + ",\"pass\": \"" + smsSetting.SmsSecret + "\"" +
				                     ",\"fromNum\" : " + "03000505".TrimStart(new[] {'0'}) + "" + ",\"toNum\": " + mobileNumber + "" + ",\"patternCode\": \" " +
				                     smsSetting.PatternCode + "\"" + ",\"inputData\" : [{\"verification-code\":" + message + "}]}", ParameterType.RequestBody);

				client.Execute(request);
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
					data = new {title = dto.Title, content = dto.Message},
					filters = new {userId = dto.UserIds}
				};
				request.AddJsonBody(body);

				IRestResponse i = await new RestClient("https://api.pushe.co/v2/messaging/notifications/").ExecuteAsync(request);
				break;
			}
		}
		return new GenericResponse();
	}
}

public class NotificationCreateDto {
	public IEnumerable<string> UserIds { get; set; }
	public string Title { get; set; }
	public string Message { get; set; }
}