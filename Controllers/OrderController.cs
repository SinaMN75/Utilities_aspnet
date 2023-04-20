namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[AllowAnonymous]
public class OrderController : BaseApiController {
	private readonly IOrderRepository _repository;

	public OrderController(IOrderRepository repository) => _repository = repository;

	[HttpPost]
	public async Task<ActionResult<GenericResponse<OrderReadDto?>>> Create(OrderCreateUpdateDto dto) => Result(await _repository.Create(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<OrderReadDto?>>> Update(OrderCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<OrderReadDto>>> Filter(OrderFilterDto dto) => Result(_repository.Filter(dto));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderReadDto>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse<OrderReadDto>>> Delete(Guid id) => Result(await _repository.Delete(id));

	[HttpPost("CreateOrderDetailToOrder")]
	public async Task<ActionResult<GenericResponse<OrderReadDto>>> CreateOrderDetailToOrder(OrderDetailCreateUpdateDto dto)
		=> Result(await _repository.CreateOrderDetail(dto));

	[HttpDelete("DeleteOrderDetail")]
	public async Task<ActionResult<GenericResponse<OrderReadDto>>> DeleteOrderDetail(Guid id) => Result(await _repository.DeleteOrderDetail(id));
}