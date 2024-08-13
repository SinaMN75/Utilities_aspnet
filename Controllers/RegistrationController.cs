namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/registration")]
[Authorize]
[ApiKey]
public class RegistrationController(IRegistrationRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<RegistrationEntity>>> Create(RegistrationCreateDto dto, CancellationToken ct) => Result(await repository.Create(dto, ct));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<RegistrationEntity>>> Update(RegistrationUpdateDto dto, CancellationToken ct) => Result(await repository.Update(dto, ct));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<RegistrationEntity>>>> Filter(RegistrationFilterDto dto) => Result(await repository.Filter(dto));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}