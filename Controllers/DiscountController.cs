namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountController(IDiscountRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> Create(DiscountEntity dto) => Result(await repository.Create(dto));

	[Authorize]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<DiscountEntity>>> Filter(DiscountFilterDto dto) => Result(repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> Update(DiscountEntity dto) => Result(await repository.Update(dto));

	[HttpDelete("{id}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id) => Result(await repository.Delete(id));

	[HttpGet("{code}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> ReadDiscountCode(string code) => Result(await repository.ReadDiscountCode(code));
}