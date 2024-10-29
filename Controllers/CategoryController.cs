using Microsoft.AspNetCore.OutputCaching;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class CategoryController(ICategoryRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Create(CategoryCreateDto dto, CancellationToken ct) =>
		Result(await repository.Create(dto, ct));

	[HttpPost("BulkCreate")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<CategoryEntity>>>> BulkCreate(IEnumerable<CategoryCreateDto> dto, CancellationToken ct) =>
		Result(await repository.BulkCreate(dto, ct));
	
	[HttpPost("ImportFromExcel")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<CategoryEntity>>>> ImportFromExcel(IFormFile file, CancellationToken ct) =>
		Result(await repository.ImportFromExcel(file, ct));

	[HttpPost("Filter")]
	[OutputCache(Duration=60, PolicyName = "default")]
	public async Task<ActionResult<GenericResponse<IQueryable<CategoryEntity>>>> Filter(CategoryFilterDto dto) => Result(await repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<CategoryEntity>>> Update(CategoryUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}