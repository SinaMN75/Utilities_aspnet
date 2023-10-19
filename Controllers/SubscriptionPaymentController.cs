namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionPaymentController(ISubscriptionPaymentRepository subscriptionPaymentRepository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<SubscriptionPaymentEntity>>> Create(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct) =>
		Result(await subscriptionPaymentRepository.Create(dto, ct));

	[HttpPost("Filter")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<SubscriptionPaymentEntity>>> Filter(SubscriptionPaymentFilter dto) => Result(subscriptionPaymentRepository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<SubscriptionPaymentEntity>>> Update(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct) =>
		Result(await subscriptionPaymentRepository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await subscriptionPaymentRepository.Delete(id, ct));
}