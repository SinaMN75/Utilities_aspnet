namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : BaseApiController {
	private readonly ICategoryRepository _repository;

	public CategoryController(ICategoryRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Create(CategoryCreateUpdateDto dto) => Result(await _repository.Create(dto));

	[HttpPost("BulkCreate")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<IEnumerable<CategoryEntity>>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto) =>
		Result(await _repository.BulkCreate(dto));

	[HttpGet]
	[OutputCache(PolicyName = "default")]
	public ActionResult<GenericResponse<IQueryable<CategoryEntity>>> Read([FromQuery] CategoryFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<CategoryEntity>>> Filter(CategoryFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Update(CategoryCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> Delete(Guid id) => Result(await _repository.Delete(id));
}