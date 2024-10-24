﻿namespace Utilities_aspnet.Repositories;

public interface ISmsNotificationRepository {
	Task<GenericResponse> SendSms(string mobileNumber, string template, string param1, string? param2 = null, string? param3 = null);
	public Task<GenericResponse> SendNotification(NotificationCreateDto dto);
}

public class SmsNotificationRepository(IConfiguration config) : ISmsNotificationRepository {
	public async Task<GenericResponse> SendSms(
		string mobileNumber,
		string template,
		string param1,
		string? param2 = null,
		string? param3 = null
	) {
		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);
		SmsPanelSettings smsSetting = appSettings.SmsPanelSettings;

		switch (smsSetting.Provider) {
			case "ghasedak": {
				RestRequest request = new(Method.POST);
				request.AddHeader("apikey", smsSetting.SmsApiKey!);
				request.AddParameter("receptor", mobileNumber);
				request.AddParameter("type", 1);
				request.AddParameter("template", smsSetting.PatternCode!);
				request.AddParameter("param1", param1);
				if (param2 != null) request.AddParameter("param2", param2);
				if (param3 != null) request.AddParameter("param3", param3);
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
				                    ",\"inputData\" : [{\"verification-code\": \"" + param1 + "\"}]}");
				await new RestClient("http://ippanel.com/api/select").ExecuteAsync(request);
				break;
			}
			case "kavenegar": {
				RestRequest request = new(Method.POST);
				request.AddHeader("apikey", smsSetting.SmsApiKey!);
				request.AddParameter("receptor", mobileNumber);
				request.AddParameter("token", param1);
				if (param2 != null) request.AddParameter("token2", param2);
				if (param3 != null) request.AddParameter("token3", param3);
				request.AddParameter("template", template);
				await new RestClient($"https://api.kavenegar.com/v1/{smsSetting.SmsApiKey}/verify/lookup.json").ExecuteAsync(request);
				break;
			}
		}

		return new GenericResponse();
	}

	public async Task<GenericResponse> SendNotification(NotificationCreateDto dto) {
		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);
		PushNotificationSetting setting = appSettings.PushNotificationSetting;

		switch (setting.Provider) {
			case "pushe": {
				RestRequest request = new(Method.POST);
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("Authorization", "Token " + setting.Token);
				var body = new {
					app_ids = setting.AppId,
					data = new {
						title = NotificationCreateDto.Title,
						content = NotificationCreateDto.Content,
						bigContent = NotificationCreateDto.BigContent,
						action = new { action_type = NotificationCreateDto.ActionType, url = NotificationCreateDto.Url }
					},
					is_draft = false,
					filter = new { custom_id = dto.UserIds }
				};
				request.AddJsonBody(body);
				await new RestClient("https://api.pushe.co/v2/messaging/notifications/").ExecuteAsync(request);
				break;
			}
			case "firebase": {
				RestRequest request = new(Method.POST);
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("Authorization", "Bearer " + setting.Token);
				var body = new {
					appId = setting.AppId,
					title = NotificationCreateDto.Title,
					body = NotificationCreateDto.Body,
					content = NotificationCreateDto.Content,
					token = dto.FcmToken
				};
				request.AddJsonBody(body);
				await new RestClient("https://fcm.sinamn75.com/send").ExecuteAsync(request);
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