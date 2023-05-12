namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/address")]
public class AddressController : BaseApiController {
	private readonly IAddressRepository _repository;
	public AddressController(IAddressRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Create(AddressCreateUpdateDto UserAddressDto)
		=> Result(await _repository.Create(UserAddressDto));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<AddressEntity>>> Update(AddressCreateUpdateDto UserAddressDto)
		=> Result(await _repository.Update(UserAddressDto));

	[HttpGet("ReadMyAddresses")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[OutputCache(PolicyName = "10s")]
	public async Task<ActionResult<GenericResponse<IEnumerable<AddressEntity>>>> ReadUserAddresses() => Result(await _repository.GetMyAddresses());

	[HttpPut("Delete")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid addressId) => Result(await _repository.DeleteAddress(addressId));
}