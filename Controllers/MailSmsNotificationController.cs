namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailSmsNotificationController(ISmsNotificationRepository repository) : BaseApiController {
	[HttpPost("SendNotification")]
	public async Task<ActionResult<GenericResponse>> SendNotification(NotificationCreateDto dto) => Result(await repository.SendNotification(dto));

	[HttpPost("SendOtpSms")]
	public async Task<ActionResult<GenericResponse>> SendOtpSms(SendOtpSmsDto dto) =>
		Result(await repository.SendSms(dto.MobileNumber, dto.Param1, dto.Template, dto.Param2, dto.Param3));
}

public class SendOtpSmsDto {
	public required string MobileNumber { get; set; }
	public required string Param1 { get; set; }
	public required string Template { get; set; }
	public string? Param2 { get; set; }
	public string? Param3 { get; set; }
}