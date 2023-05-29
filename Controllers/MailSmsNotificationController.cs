namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailSmsNotificationController : BaseApiController {
	private readonly ISmsNotificationRepository _repository;

	public MailSmsNotificationController(ISmsNotificationRepository repository) { _repository = repository; }

	[HttpPost("SendNotification")]
	public async Task<ActionResult<GenericResponse>> SendNotification(NotificationCreateDto dto) { return Result(await _repository.SendNotification(dto)); }
}