namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/address")]
public class AddressController : BaseApiController {
	private readonly IAddressRepository _repository;

	public AddressController(IAddressRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateUpdateDto dto, CancellationToken ct) =>
		Result(await _repository.Create(dto, ct));

	[HttpGet]
	[Authorize]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<AddressEntity>>> Read([FromQuery] AddressFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressCreateUpdateDto dto, CancellationToken ct) =>
		Result(await _repository.Update(dto, ct));

	[HttpPost("Filter")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto) => Result(_repository.Filter(dto));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));
}