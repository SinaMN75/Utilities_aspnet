using PostmarkDotNet;
using System.Net.Http.Headers;

namespace Utilities_aspnet.Repositories;

public interface ISmsNotificationRepository {
	void SendSms(string mobileNumber, string message);
	public Task<GenericResponse> SendNotification(NotificationCreateDto dto);
	public Task<GenericResponse> SendMail(SendMailDto dto);
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

    public async Task<GenericResponse> SendNotification(NotificationCreateDto dto)
    {
        AppSettings appSettings = new();
        _config.GetSection("AppSettings").Bind(appSettings);
        PushNotificationSetting setting = appSettings.PushNotificationSetting;

        switch (setting.Provider)
        {
            case "pushe":
                {
                    var jsModel = new
                    {
                        app_ids = "6g035k5283mm7z5g",
                        data = new
                        {
                            title = "backEnd",
                            content = "test backend baraye double check"
                        }
                    };
                    string s = JsonConvert.SerializeObject(jsModel, Formatting.Indented);
                    ByteArrayContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(s));
                    using HttpClient client = new HttpClient();
                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://api.pushe.co/v2/messaging/notifications/"),
                        Content = content
                    };
                    httpRequestMessage.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    httpRequestMessage.Headers.Add("Authorization", "Token 83080b0bb6511721efa6e59ab2ea8a11e2689797");
                    var d = await (await client.SendAsync(httpRequestMessage)).Content.ReadAsStringAsync();

                    break;
                }
                //       case "pushe":
                //           {
                //               RestRequest request = new(Method.POST);
                //               request.AddHeader("Authorization", "Token " + setting.Token);
                //               request.AddHeader("Content-Type", "application/json");
                //               var body = new
                //               {
                //                   app_ids = setting.AppId,
                //                   data = new { title = dto.Title, content = dto.Message }
                //               };
                //               /* 
                //                  ,
                //                   filters = new {userId = dto.UserIds}
                //*/
                //               request.AddJsonBody(body);

                //               var d = new RestClient("https://api.pushe.co/v2/messaging/notifications/").Execute(request);
                //               Console.WriteLine($"--------------***********///////////// {d}");
                //               break;
                //           }
        }
        return new GenericResponse();
    }
    public async Task<GenericResponse> SendMail(SendMailDto dto) {
		try {
			PostmarkMessage message = new() {
				To = dto.To,
				From = "Support@toubasources.com",
				Subject = "Touba Technical Mail",
				TextBody = dto.PlainText,
			};
			PostmarkClient client = new PostmarkClient("00ed8f38-a980-4201-bddd-6621fb77eaa6");
			PostmarkResponse? sendResult = await client.SendMessageAsync(message);

			return sendResult.Status == PostmarkStatus.Success ? new GenericResponse() : new GenericResponse(UtilitiesStatusCodes.Unhandled);

			#region SendGrid

			#endregion
		}
		catch (Exception ex) {
			return new GenericResponse(UtilitiesStatusCodes.Unhandled, ex.ToString());
		}
	}
}

public class NotificationCreateDto {
	public IEnumerable<string> UserIds { get; set; }
	public string Title { get; set; }
	public string Message { get; set; }
}
