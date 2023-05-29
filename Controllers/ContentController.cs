namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[OutputCache(PolicyName = "default")]
public class ContentController : BaseApiController {
	private readonly IContentRepository _repository;

	public ContentController(IContentRepository repository) { _repository = repository; }

	[HttpGet]
	public ActionResult<GenericResponse<IQueryable<ContentEntity>>> Read() { return Result(_repository.Read()); }

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Create(ContentCreateUpdateDto dto) { return Result(await _repository.Create(dto)); }

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Update(ContentCreateUpdateDto dto) { return Result(await _repository.Update(dto)); }

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> Delete(Guid id) { return Result(await _repository.Delete(id)); }
}