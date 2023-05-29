namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountController : BaseApiController {
	private readonly IDiscountRepository _repository;

	public DiscountController(IDiscountRepository repository) { _repository = repository; }

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> Create(DiscountEntity dto) { return Result(await _repository.Create(dto)); }

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<DiscountEntity>>> Filter(DiscountFilterDto dto) { return Result(_repository.Filter(dto)); }

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> Update(DiscountEntity dto) { return Result(await _repository.Update(dto)); }

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> Delete(Guid id) { return Result(await _repository.Delete(id)); }

	[HttpGet("{code}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<DiscountEntity>>> ReadDiscountCode(string code) { return Result(await _repository.ReadDiscountCode(code)); }
}