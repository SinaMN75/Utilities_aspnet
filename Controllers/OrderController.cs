namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderRepository repository) : BaseApiController {
	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Update(OrderCreateUpdateDto dto) => Result(await repository.Update(dto));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<OrderEntity>>>> Filter(OrderFilterDto dto) => Result(await repository.Filter(dto));

	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> ReadById(Guid id) => Result(await repository.ReadById(id));

	[HttpPost("Vote")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Vote(OrderVoteDto dto) => Result(await repository.Vote(dto));

	[HttpDelete("{id}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> Delete(Guid id) => Result(await repository.Delete(id));

	[HttpPost("CreateUpdateOrderDetail")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) =>
		Result(await repository.CreateUpdateOrderDetail(dto));
	
	[HttpPost("CreateReservationOrder")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateReservationOrder(ReserveCreateUpdateDto dto) =>
		Result(await repository.CreateReservationOrder(dto));		
	
	[HttpPost("CreateChairReservationOrder")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateChairReservationOrder(ReserveChairCreateUpdateDto dto) =>
		Result(await repository.CreateChairReservationOrder(dto));	
}