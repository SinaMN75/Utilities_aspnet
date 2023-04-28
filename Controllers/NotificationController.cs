namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : BaseApiController {
	private readonly INotificationRepository _repository;

	public NotificationController(INotificationRepository repository) => _repository = repository;

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[OutputCache(PolicyName = "default")]
	[HttpGet]
	public ActionResult<GenericResponse<IQueryable<NotificationEntity>>> Read() => Result(_repository.Read());

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[OutputCache(PolicyName = "default")]
	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<IQueryable<NotificationEntity>>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[HttpPost]
	public async Task<ActionResult<GenericResponse>> Create(NotificationCreateUpdateDto model) => Result(await _repository.Create(model));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse> Filter(NotificationFilterDto dto) => Result(_repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[HttpPost("UpdateSeenStatus")]
	public async Task<ActionResult<GenericResponse>> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus)
		=> Result(await _repository.UpdateSeenStatus(ids, seenStatus));
}