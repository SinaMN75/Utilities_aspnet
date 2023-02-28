namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GlobalSearchController : BaseApiController {
	private readonly IGlobalSearchRepository _repository;

	public GlobalSearchController(IGlobalSearchRepository repository) {
		_repository = repository;
	}

	[HttpPost]
	public ActionResult<GenericResponse<GlobalSearchDto>> Filter(GlobalSearchParams filter) {
		GenericResponse<GlobalSearchDto> i = _repository.Filter(filter, User.Identity?.Name);
		return Result(i);
	}
}