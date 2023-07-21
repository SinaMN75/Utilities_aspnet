namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : BaseApiController {
	private readonly IOrderRepository _repository;

	public OrderController(IOrderRepository repository) => _repository = repository;

	[HttpPut]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Update(OrderCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<OrderEntity>>>> Filter(OrderFilterDto dto) => Result(await _repository.Filter(dto));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[HttpPost("Vote")]
	public async Task<ActionResult<GenericResponse>> Vote(OrderVoteDto dto) => Result(await _repository.Vote(dto));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> Delete(Guid id) => Result(await _repository.Delete(id));

	[HttpPost("CreateUpdateOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) =>
		Result(await _repository.CreateUpdateOrderDetail(dto));
}