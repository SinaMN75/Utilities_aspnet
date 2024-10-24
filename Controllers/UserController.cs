namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/user")]
[ApiKey]
public class UserController(IUserRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Create(UserCreateUpdateDto dto) => Result(await repository.Create(dto));
	
	[HttpPost("LoginWithPassword")]
	public async Task<ActionResult<GenericResponse>> LoginWithPassword(LoginWithPasswordDto dto) => Result(await repository.LoginWithPassword(dto));

	[HttpPost("GetVerificationCodeForLogin")]
	public async Task<ActionResult<GenericResponse<UserEntity>>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto) =>
		Result(await repository.GetVerificationCodeForLogin(dto));

	[HttpPost("VerifyCodeForLogin")]
	public async Task<ActionResult<GenericResponse>> VerifyCodeForLogin(VerifyMobileForLoginDto dto) => Result(await repository.VerifyCodeForLogin(dto));

	[HttpPost("GetTokenForTest/{mobile}")]
	public async Task<ActionResult<GenericResponse>> GetTokenForTest(string? mobile) => Result(await repository.GetTokenForTest(mobile));

	[Authorize]
	[HttpDelete]
	public async Task<ActionResult<GenericResponse>> Delete(string id, CancellationToken ct) => Result(await repository.Delete(id,ct));

	[Authorize]
	[AllowAnonymous]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> Filter(UserFilterDto dto) => Result(await repository.Filter(dto));
	
	[Authorize]
	[AllowAnonymous]
	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<UserEntity?>>> ReadById(string id) => Result(await repository.ReadById(id));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Update(UserCreateUpdateDto dto) => Result(await repository.Update(dto));
	
	[HttpPost("Authorize")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Authorize(AuthorizeUserDto dto) => Result(await repository.Authorize(dto));
	
	[HttpPost("Subscribe")]
	public async Task<ActionResult<GenericResponse>> Subscribe(string userId, Guid contentId, string transactionRefId) => 
		Result(await repository.Subscribe(userId, contentId, transactionRefId));
}