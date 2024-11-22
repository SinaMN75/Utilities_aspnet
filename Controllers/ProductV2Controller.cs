namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductV2Controller(IProductRepository repository, IOutputCacheStore store) : BaseApiController {
	[Authorize]
	[HttpPost]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> Create(ProductCreateUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("product", ct);
		return Result(await repository.Create(dto, ct));
	}

	[Authorize]
	[AllowAnonymous]
	[OutputCache(PolicyName = "default", Tags = ["product"])]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<ProductEntity>>>> Filter(ProductFilterDto dto) {
		return Result(await repository.Filter(dto));
	}

	[Authorize]
	[AllowAnonymous]
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> ReadById(Guid id, CancellationToken ct) => 
		Result(await repository.ReadById(id, ct));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> Update(ProductCreateUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("product", ct);
		return Result(await repository.Update(dto, ct));
	}

	[Authorize]
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) {
		await store.EvictByTagAsync("product", ct);
		return Result(await repository.Delete(id, ct));
	}

	[Authorize]
	[HttpPost("CreateReaction")]
	public async Task<ActionResult<GenericResponse>> CreateReaction(ReactionCreateUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("product", ct);
		return Result(await repository.CreateReaction(dto));
	}
}