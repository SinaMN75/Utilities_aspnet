namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionPaymentController : BaseApiController {
	private readonly ISubscriptionPaymentRepository _repository;
	public SubscriptionPaymentController(ISubscriptionPaymentRepository subscriptionPaymentRepository) => _repository = subscriptionPaymentRepository;

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<SubscriptionPaymentEntity>>> Create(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct) =>
		Result(await _repository.Create(dto, ct));

	[HttpPost("Filter")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<SubscriptionPaymentEntity>>> Filter(SubscriptionPaymentFilter dto) => Result(_repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<SubscriptionPaymentEntity>>> Update(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct) =>
		Result(await _repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));
}