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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("Filter")]
    public ActionResult<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto) => Result(_repository.Filter(dto));
    
	[HttpDelete("{addressId:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid addressId) => Result(await _repository.Delete(addressId));
}