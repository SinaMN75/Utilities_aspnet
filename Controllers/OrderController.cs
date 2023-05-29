namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[AllowAnonymous]
public class OrderController : BaseApiController {
	private readonly IOrderRepository _repository;

	public OrderController(IOrderRepository repository) { _repository = repository; }

	[HttpPost]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Create(OrderCreateUpdateDto dto) { return Result(await _repository.Create(dto)); }

	[HttpPut]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Update(OrderCreateUpdateDto dto) { return Result(await _repository.Update(dto)); }

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<OrderEntity>>> Filter(OrderFilterDto dto) { return Result(_repository.Filter(dto)); }

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> ReadById(Guid id) { return Result(await _repository.ReadById(id)); }

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> Delete(Guid id) { return Result(await _repository.Delete(id)); }

	[HttpPost("CreateOrderDetailToOrder")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateOrderDetailToOrder(OrderDetailCreateUpdateDto dto) {
		return Result(await _repository.CreateOrderDetail(dto));
	}

	[HttpDelete("DeleteOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> DeleteOrderDetail(Guid id) { return Result(await _repository.DeleteOrderDetail(id)); }
}