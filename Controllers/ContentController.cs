using Microsoft.AspNetCore.OutputCaching;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class ContentController(IContentRepository repository, IOutputCacheStore store) : BaseApiController {
	
	[OutputCache(PolicyName = "default", Tags = ["content"])]
	[HttpGet]
	public ActionResult<GenericResponse<IQueryable<ContentEntity>>> Read() {
		return Result(repository.Read());
	}

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Create(ContentCreateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("content", ct);
		return Result(await repository.Create(dto, ct));
	}

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ContentEntity>>> Update(ContentUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("content", ct);
		return Result(await repository.Update(dto, ct));
	}

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) {
		await store.EvictByTagAsync("content", ct);
		return Result(await repository.Delete(id, ct));
	}
}