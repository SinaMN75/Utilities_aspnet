namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : BaseApiController {
	private readonly IUserRepository _repository;

	public UserController(IUserRepository repository) => _repository = repository;

	[HttpPost("Register")]
	public async Task<ActionResult<GenericResponse>> Register(RegisterDto dto) => Result(await _repository.Register(dto));

	[HttpPost("LoginWithPassword")]
	public async Task<ActionResult<GenericResponse>> LoginWithPassword(LoginWithPasswordDto dto) => Result(await _repository.LoginWithPassword(dto));

	[HttpPost("GetVerificationCodeForLogin")]
	public async Task<ActionResult<GenericResponse>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto)
		=> Result(await _repository.GetVerificationCodeForLogin(dto));

	[HttpPost("VerifyCodeForLogin")]
	public async Task<ActionResult<GenericResponse>> VerifyCodeForLogin(VerifyMobileForLoginDto dto) => Result(await _repository.VerifyCodeForLogin(dto));

	[HttpPost("GetTokenForTest/{mobile}")]
	public async Task<ActionResult<GenericResponse>> GetTokenForTest(string? mobile) => Result(await _repository.GetTokenForTest(mobile));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> Filter(UserFilterDto dto) => Result(await _repository.Filter(dto));
	
	[HttpGet]
	[OutputCache(PolicyName = "default")]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> Read([FromQuery] UserFilterDto dto) => Result(await _repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpGet("{id}")]
	[OutputCache(PolicyName = "default")]
	public async Task<ActionResult<GenericResponse<UserEntity?>>> ReadById(string id) => Result(await _repository.ReadById(id));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Update(UserCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpGet("ReadMyBlockList")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[OutputCache(PolicyName = "10s")]
	public async Task<ActionResult<GenericResponse<IQueryable<UserEntity>>>> ReadMine() => Result(await _repository.ReadMyBlockList());

	[HttpPost("ToggleBlock")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Create(string userId) => Result(await _repository.ToggleBlock(userId));

	[HttpPost("TransferWalletToWallet")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> TransferWalletToWallet(TransferFromWalletToWalletDto dto)
		=> Result(await _repository.TransferWalletToWallet(dto));

	[HttpPost("AddUserAddresses")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [OutputCache(PolicyName = "10s")]
    public async Task<ActionResult<GenericResponse<UserAddresses>>> AddUserAddress(UserAddressDto UserAddressDto) => Result(await _repository.AddUserAddress(UserAddressDto));

    [HttpGet("ReadMyAddresses")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [OutputCache(PolicyName = "10s")]
    public async Task<ActionResult<GenericResponse<IEnumerable<UserAddresses>>>> GetUserAddresses() => Result(await _repository.GetMyAddresses());
}