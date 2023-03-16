namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GlobalSearchController : BaseApiController {
	private readonly IGlobalSearchRepository _repository;

	public GlobalSearchController(IGlobalSearchRepository repository) {
		_repository = repository;
	}

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost]
	public async Task<ActionResult<GenericResponse<GlobalSearchDto>>> Filter(GlobalSearchParams filter)
		=> Result(await _repository.Filter(filter, User.Identity?.Name));
}