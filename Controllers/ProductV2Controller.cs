namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductV2Controller(IProductRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> Create(ProductCreateUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Create(dto, ct));

	[Authorize]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<ProductEntity>>>> Filter(ProductFilterDto dto) => Result(await repository.Filter(dto));

	[Authorize]
	[AllowAnonymous]
	[HttpGet]
	public async Task<ActionResult<GenericResponse<IQueryable<ProductEntity>>>> Read([FromQuery] ProductFilterDto dto) => Result(await repository.Filter(dto));

	[Authorize]
	[AllowAnonymous]
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> ReadById(Guid id, CancellationToken ct) => Result(await repository.ReadById(id, ct));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ProductEntity>>> Update(ProductCreateUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));

	[HttpPost("CreateReaction")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> CreateReaction(ReactionCreateUpdateDto dto) => Result(await repository.CreateReaction(dto));

	[HttpGet("ReadReactions/{id:guid}")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<ReactionEntity>>> ReadReactionsById(Guid id) => Result(repository.ReadReactionsById(id));

	[HttpPost("FilterReaction")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<ReactionEntity>>> FilterReaction(ReactionFilterDto dto) => Result(repository.FilterReaction(dto));

	[HttpGet("GetMyCustomersPerProduct/{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IQueryable<CustomersPaymentPerProduct>?>>> GetMyCustomersPerProduct(Guid id) =>
		Result(await repository.GetMyCustomersPerProduct(id));
}