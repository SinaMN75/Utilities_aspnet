namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class MailSmsController : BaseApiController {
	private readonly ISmsNotificationRepository _repository;

	public MailSmsController(ISmsNotificationRepository repository) => _repository = repository;

	[HttpPost("SendMail")]
	public async Task<ActionResult<GenericResponse>> SendMail(SendMailDto dto) => Result(await _repository.SendMail(dto));
	
	[HttpPost("SendNotification")]
	public ActionResult<GenericResponse> SendNotification(NotificationCreateDto dto) => Result(_repository.SendNotification(dto));
}