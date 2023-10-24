namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/address")]
public class AddressController(IAddressRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateDto dto, CancellationToken ct) =>
		Result(await repository.Create(dto, ct));

	[HttpGet]
	[Authorize]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<AddressEntity>>> Read([FromQuery] AddressFilterDto dto) => Result(repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Update(dto, ct));

	[HttpPost("Filter")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto) => Result(repository.Filter(dto));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}