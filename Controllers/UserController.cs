namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/user")]
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
	[AllowAnonymous]
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<UserEntity>>> Filter(UserFilterDto dto) => Result(repository.Filter(dto));

	[HttpGet]
	public ActionResult<GenericResponse<IEnumerable<UserEntity>>> Read([FromQuery] UserFilterDto dto) => Result(repository.Filter(dto));

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

	[HttpGet("ReadMyBlockList")]
	public async Task<ActionResult<GenericResponse<IQueryable<UserEntity>>>> ReadMyBlockList() => Result(await repository.ReadMyBlockList());

	[HttpPost("ToggleBlock/{id}")]
	public async Task<ActionResult<GenericResponse>> Block(string id) => Result(await repository.ToggleBlock(id));
	
	[HttpPost("Subscribe")]
	public async Task<ActionResult<GenericResponse>> Subscribe(string userId, Guid contentId, string transactionRefId) => 
		Result(await repository.Subscribe(userId, contentId, transactionRefId));
}