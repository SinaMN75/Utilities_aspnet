namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController(IOrderRepository repository) : BaseApiController {
	[HttpPut]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Update(OrderCreateUpdateDto dto) => Result(await repository.Update(dto));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<OrderEntity>>>> Filter(OrderFilterDto dto) => Result(await repository.Filter(dto));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> ReadById(Guid id) => Result(await repository.ReadById(id));

	[HttpPost("Vote")]
	public async Task<ActionResult<GenericResponse>> Vote(OrderVoteDto dto) => Result(await repository.Vote(dto));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> Delete(Guid id) => Result(await repository.Delete(id));

	[HttpPost("CreateUpdateOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) =>
		Result(await repository.CreateUpdateOrderDetail(dto));
	
	[HttpPost("CreateReservationOrder")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateReservationOrder(ReserveCreateUpdateDto dto) =>
		Result(await repository.CreateReservationOrder(dto));	
	
	[HttpPost("CreateReservationOrder2")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateReservationOrder2(ReserveCreateUpdateDto dto) =>
		Result(await repository.CreateReservationOrder2(dto));
}