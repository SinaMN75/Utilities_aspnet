namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[AllowAnonymous]
public class OrderController : BaseApiController {
	private readonly IOrderRepository _repository;

	public OrderController(IOrderRepository repository) => _repository = repository;

	[HttpPost]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Create(OrderCreateUpdateDto dto) => Result(await _repository.Create(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Update(OrderCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<OrderEntity>>> Filter(OrderFilterDto dto) => Result(_repository.Filter(dto));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> Delete(Guid id) => Result(await _repository.Delete(id));

	[HttpPost("CreateOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateOrderDetailToOrder(OrderDetailCreateUpdateDto dto) =>
		Result(await _repository.CreateOrderDetail(dto));

	[HttpPut("UpdateOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> UpdateOrderDetailToOrder(OrderDetailCreateUpdateDto dto) =>
		Result(await _repository.UpdateOrderDetail(dto));

	[HttpDelete("DeleteOrderDetail/{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> DeleteOrderDetail(Guid id) => Result(await _repository.DeleteOrderDetail(id));

	[HttpPost("CreateUpdateOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) => Result(await _repository.CreateUpdateOrderDetail(dto));
}