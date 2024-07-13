namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/address")]
[Authorize]
[ApiKey]
public class AddressController(IAddressRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateDto dto, CancellationToken ct) => Result(await repository.Create(dto, ct));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressUpdateDto dto, CancellationToken ct) => Result(await repository.Update(dto, ct));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<AddressEntity>>>> Filter(AddressFilterDto dto) => Result(await repository.Filter(dto));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}