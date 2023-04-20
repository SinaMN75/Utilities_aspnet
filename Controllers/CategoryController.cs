namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : BaseApiController {
	private readonly ICategoryRepository _repository;

	public CategoryController(ICategoryRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<CategoryReadDto>>> Create(CategoryCreateUpdateDto dto) => Result(await _repository.Create(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<CategoryReadDto>>> Filter(CategoryFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<CategoryReadDto>>> Update(CategoryCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> Delete(Guid id) => Result(await _repository.Delete(id));
}