namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : BaseApiController {
	private readonly ICategoryRepository _repository;

	public CategoryController(ICategoryRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Create(CategoryCreateUpdateDto dto, CancellationToken ct) => Result(await _repository.Create(dto, ct));

	[HttpPost("BulkCreate")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<CategoryEntity>>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto, CancellationToken ct) =>
		Result(await _repository.BulkCreate(dto, ct));

	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<CategoryEntity>>> Read([FromQuery] CategoryFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<CategoryEntity>>> Filter(CategoryFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Update(CategoryCreateUpdateDto dto, CancellationToken ct) => Result(await _repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));
}