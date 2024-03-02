namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotificationRepository repository) : BaseApiController {
	[Authorize]
	[AllowAnonymous]
	[HttpGet]
	public ActionResult<GenericResponse<IQueryable<NotificationEntity>>> Read() => Result(repository.Read());

	[Authorize]
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<IQueryable<NotificationEntity>>>> ReadById(Guid id) => Result(await repository.ReadById(id));

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<GenericResponse>> Create(NotificationCreateUpdateDto model) => Result(await repository.Create(model));

	[Authorize]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse> Filter(NotificationFilterDto dto) => Result(repository.Filter(dto));

	[Authorize]
	[HttpPost("UpdateSeenStatus")]
	public async Task<ActionResult<GenericResponse>> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus) =>
		Result(await repository.UpdateSeenStatus(ids, seenStatus));
}