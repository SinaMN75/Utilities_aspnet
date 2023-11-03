namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/address")]
[Authorize]
public class AddressController(IAddressRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateDto dto, CancellationToken ct) =>
		Result(await repository.Create(dto, ct));

	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public async Task<ActionResult<GenericResponse<IQueryable<AddressEntity>>>> Read([FromQuery] AddressFilterDto dto) => Result(await repository.Filter(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Update(dto, ct));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<AddressEntity>>>> Filter(AddressFilterDto dto) => Result(await repository.Filter(dto));

	[HttpDelete("{id}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}