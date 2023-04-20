namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductV2Controller : BaseApiController {
	private readonly IProductRepository _repository;

	public ProductV2Controller(IProductRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ProductReadDto>>> Create(ProductCreateUpdateDto dto, CancellationToken ct)
		=> Result(await _repository.Create(dto, ct));

	[HttpPost("CreateWithMedia")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ProductReadDto>>> CreateWithMedia([FromForm] ProductCreateUpdateDto dto, CancellationToken ct)
		=> Result(await _repository.CreateWithFiles(dto, ct));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<ProductReadDto>>>> Filter(ProductFilterDto dto) => Result(await _repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<ProductReadDto>>> ReadById(Guid id, CancellationToken ct) => Result(await _repository.ReadById(id, ct));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ProductReadDto>>> Update(ProductCreateUpdateDto dto, CancellationToken ct)
		=> Result(await _repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));
}