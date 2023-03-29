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
	public async Task<ActionResult<GenericResponse>> GetTokenForTest(string mobile) => Result(await _repository.GetTokenForTest(mobile));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> Filter(UserFilterDto dto) => Result(await _repository.Filter(dto));

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<UserEntity?>>> ReadById(string id) => Result(await _repository.ReadById(id));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Update(UserCreateUpdateDto dto) => Result(await _repository.Update(dto));

	[HttpDelete("{id}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(string id) => Result(await _repository.Delete(id));

	[HttpGet("ReadMyBlockList")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<IQueryable<UserEntity>>>> ReadMine() => Result(await _repository.ReadMyBlockList());

	[HttpPost("ToggleBlock")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Create(string userId) => Result(await _repository.ToggleBlock(userId));
}