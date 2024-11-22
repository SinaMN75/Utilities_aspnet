namespace Utilities_aspnet.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DiscountController(IDiscountRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> Create(DiscountCreateDto dto) => Result(await repository.Create(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<DiscountEntity>>> Filter(DiscountFilterDto dto) => Result(repository.Filter(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> Update(DiscountUpdateDto dto) => Result(await repository.Update(dto));

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id) => Result(await repository.Delete(id));
}