namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class NotificationController(INotificationRepository repository) : BaseApiController {
	[Authorize]
	[HttpPost]
	public async Task<ActionResult<GenericResponse>> Create(NotificationCreateUpdateDto model) => Result(await repository.Create(model));

	[Authorize]
	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await repository.Delete(id));

	[Authorize]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse> Filter(NotificationFilterDto dto) => Result(repository.Filter(dto));

	[Authorize]
	[HttpPost("UpdateSeenStatus")]
	public async Task<ActionResult<GenericResponse>> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus) =>
		Result(await repository.UpdateSeenStatus(ids, seenStatus));
}