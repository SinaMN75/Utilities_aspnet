namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/address")]
public class AddressController : BaseApiController {
	private readonly IAddressRepository _repository;
	public AddressController(IAddressRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateUpdateDto dto) => Result(await _repository.Create(dto));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpGet("ReadMyAddresses")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public ActionResult<GenericResponse<IQueryable<AddressEntity>>> ReadUserAddresses() => Result(_repository.ReadMyAddresses());

	[HttpDelete]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid addressId) => Result(await _repository.DeleteAddress(addressId));
}