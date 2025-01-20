namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryRepository repository, IOutputCacheStore store) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Create(CategoryCreateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("category", ct);
		return Result(await repository.Create(dto, ct));
	}

	[HttpPost("BulkCreate")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<CategoryEntity>>>> BulkCreate(IEnumerable<CategoryCreateDto> dto, CancellationToken ct) {
		await store.EvictByTagAsync("category", ct);
		return Result(await repository.BulkCreate(dto, ct));
	}

	[HttpPost("Filter")]
	[OutputCache(PolicyName = "default", Tags = ["category"])]
	public async Task<ActionResult<GenericResponse<IQueryable<CategoryEntity>>>> Filter(CategoryFilterDto dto) => Result(await repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Update(CategoryUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("category", ct);
		return Result(await repository.Update(dto, ct));
	}

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) {
		await store.EvictByTagAsync("category", ct);
		return Result(await repository.Delete(id, ct));
	}
}