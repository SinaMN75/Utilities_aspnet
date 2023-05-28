namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductV2Controller : BaseApiController {
	private readonly IProductRepository _repository;

	public ProductV2Controller(IProductRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> Create(ProductCreateUpdateDto dto, CancellationToken ct)
		=> Result(await _repository.Create(dto, ct));

	[HttpPost("CreateWithMedia")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> CreateWithMedia([FromForm] ProductCreateUpdateDto dto, CancellationToken ct)
		=> Result(await _repository.CreateWithFiles(dto, ct));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<ProductEntity>>>> Filter(ProductFilterDto dto) => Result(await _repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[OutputCache(PolicyName = "default")]
	[HttpGet]
	public async Task<ActionResult<GenericResponse<IQueryable<ProductEntity>>>> Read([FromQuery] ProductFilterDto dto) => Result(await _repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> ReadById(Guid id, CancellationToken ct) => Result(await _repository.ReadById(id, ct));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> Update(ProductCreateUpdateDto dto, CancellationToken ct)
		=> Result(await _repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));

	[HttpPost("CreateReaction")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> CreateReaction(ReactionCreateUpdateDto dto) => Result(await _repository.CreateReaction(dto));

	[HttpGet("ReadReactions/{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public ActionResult<GenericResponse<IQueryable<ReactionEntity>>> ReadReactionsById(Guid id) => Result(_repository.ReadReactionsById(id));
}