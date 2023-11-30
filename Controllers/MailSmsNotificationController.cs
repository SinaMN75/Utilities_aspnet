namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailSmsNotificationController(ISmsNotificationRepository repository) : BaseApiController {
	[HttpPost("SendNotification")]
	public async Task<ActionResult<GenericResponse>> SendNotification(NotificationCreateDto dto) => Result(await repository.SendNotification(dto));

	[HttpPost("SendOtpSms")]
	public async Task<ActionResult<GenericResponse>> SendOtpSms(string mobileNumber,
		string param1,
		string? template,
		string? param2 = null,
		string? param3 = null
	) =>
		Result(await repository.SendSms(mobileNumber, template, param1, param2, param3));
}