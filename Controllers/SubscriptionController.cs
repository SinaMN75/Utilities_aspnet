namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionController(ISubscriptionRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<SubscriptionEntity>>> Create(SubscriptionCreateDto dto, CancellationToken ct) => Result(await repository.Create(dto, ct));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<SubscriptionEntity>>> Update(SubscriptionUpdateDto dto, CancellationToken ct) => Result(await repository.Update(dto, ct));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<SubscriptionEntity>>>> Filter(SubscriptionFilterDto dto) => Result(await repository.Filter(dto));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}