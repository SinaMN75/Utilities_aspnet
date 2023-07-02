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
	public async Task<ActionResult<GenericResponse>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto) =>
		Result(await _repository.GetVerificationCodeForLogin(dto));

	[HttpPost("VerifyCodeForLogin")]
	public async Task<ActionResult<GenericResponse>> VerifyCodeForLogin(VerifyMobileForLoginDto dto) => Result(await _repository.VerifyCodeForLogin(dto));

	[HttpPost("GetTokenForTest/{mobile}")]
	public async Task<ActionResult<GenericResponse>> GetTokenForTest(string? mobile) => Result(await _repository.GetTokenForTest(mobile));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<UserEntity>>> Filter(UserFilterDto dto) => Result(_repository.Filter(dto));

	[HttpGet]
	public ActionResult<GenericResponse<IEnumerable<UserEntity>>> Read([FromQuery] UserFilterDto dto) => Result(_repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<UserEntity?>>> ReadById(string id) => Result(await _repository.ReadById(id));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Update(UserCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpPost("TransferWalletToWallet")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> TransferWalletToWallet(TransferFromWalletToWalletDto dto, CancellationToken ct) =>
		Result(await _repository.TransferWalletToWallet(dto,ct));

	[HttpPost("Authorize")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Authorize(AuthorizeUserDto dto) => Result(await _repository.Authorize(dto));
	
	[HttpGet("ReadMyBlockList")]
	public async Task<ActionResult<GenericResponse<IQueryable<UserEntity>>>> ReadMyBlockList() => Result(await _repository.ReadMyBlockList());

	[HttpPost("ToggleBlock/{id}")]
	public async Task<ActionResult<GenericResponse>> Block(string id) => Result(await _repository.ToggleBlock(id));
}