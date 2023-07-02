namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController : BaseApiController {
	private readonly IContentRepository _repository;

	public ContentController(IContentRepository repository) => _repository = repository;

	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<ContentEntity>>> Read() => Result(_repository.Read());

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Create(ContentCreateUpdateDto dto, CancellationToken ct) =>
		Result(await _repository.Create(dto, ct));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Update(ContentCreateUpdateDto dto, CancellationToken ct) => Result(await _repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));
}