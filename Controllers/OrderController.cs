namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrderController : BaseApiController {
	private readonly IOrderRepository _repository;

	public OrderController(IOrderRepository repository) => _repository = repository;

	[HttpPost]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Create(OrderCreateUpdateDto dto) => Result(await _repository.Create(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<OrderEntity?>>> Update(OrderCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<OrderEntity>>> Filter(OrderFilterDto dto) => Result(_repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> Delete(Guid id) => Result(await _repository.Delete(id));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("CreateOrderDetailToOrder")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> CreateOrderDetailToOrder(OrderDetailCreateUpdateDto dto)
		=> Result(await _repository.CreateOrderDetail(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpDelete("DeleteOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderEntity>>> DeleteOrderDetail(Guid id) => Result(await _repository.DeleteOrderDetail(id));
}