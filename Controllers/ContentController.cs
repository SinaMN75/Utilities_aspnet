namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController(IContentRepository repository, IOutputCacheStore outputCache) : BaseApiController {
	[HttpGet]
	[OutputCache(Tags = ["content"])]
	public ActionResult<GenericResponse<IQueryable<ContentEntity>>> Read() => Result(repository.Read());

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Create(ContentCreateDto dto, CancellationToken ct) {
		await outputCache.EvictByTagAsync("content", ct);
		return Result(await repository.Create(dto, ct));
	}

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Update(ContentUpdateDto dto, CancellationToken ct) {
		await outputCache.EvictByTagAsync("content", ct);
		return Result(await repository.Update(dto, ct));
	}

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) {
		await outputCache.EvictByTagAsync("content", ct);
		return Result(await repository.Delete(id, ct));
	}
}