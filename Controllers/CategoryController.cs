namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Create(CategoryCreateUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Create(dto, ct));

	[HttpPost("BulkCreate")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<CategoryEntity>>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto, CancellationToken ct) =>
		Result(await repository.BulkCreate(dto, ct));

	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<CategoryEntity>>> Read([FromQuery] CategoryFilterDto dto) => Result(repository.Filter(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<CategoryEntity>>> Filter(CategoryFilterDto dto) => Result(repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Update(CategoryCreateUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}