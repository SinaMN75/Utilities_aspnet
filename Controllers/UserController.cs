namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserRepository repository, IOutputCacheStore store) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Create(UserCreateUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.Create(dto));
	}

	[HttpPost("LoginWithPassword")]
	public async Task<ActionResult<GenericResponse>> LoginWithPassword(LoginWithPasswordDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.LoginWithPassword(dto));
	}

	[HttpPost("RefreshToken")]
	public async Task<ActionResult<GenericResponse>> RefreshToken(RefreshTokenDto dto) => Result(await repository.RefreshToken(dto));

	[HttpPost("GetVerificationCodeForLogin")]
	public async Task<ActionResult<GenericResponse<UserEntity>>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.GetVerificationCodeForLogin(dto));
	}

	[HttpPost("VerifyCodeForLogin")]
	public async Task<ActionResult<GenericResponse>> VerifyCodeForLogin(VerifyMobileForLoginDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.VerifyCodeForLogin(dto));
	}

	[HttpPost("GetTokenForTest/{mobile}")]
	public async Task<ActionResult<GenericResponse>> GetTokenForTest(string mobile, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.GetTokenForTest(mobile));
	}

	[Authorize]
	[HttpDelete]
	public async Task<ActionResult<GenericResponse>> Delete(string id, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.Delete(id, ct));
	}

	[Authorize]
	[AllowAnonymous]
	[OutputCache(PolicyName = "default", Tags = ["user"])]
	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> Filter(UserFilterDto dto) {
		return Result(await repository.Filter(dto));
	}

	[Authorize]
	[AllowAnonymous]
	[OutputCache(PolicyName = "default", Tags = ["user"])]
	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<UserEntity?>>> ReadById(string id) {
		return Result(await repository.ReadById(id));
	}

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<UserEntity>>> Update(UserCreateUpdateDto dto, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.Update(dto));
	}

	[HttpPost("Subscribe")]
	public async Task<ActionResult<GenericResponse>> Subscribe(string userId, Guid contentId, string transactionRefId, CancellationToken ct) {
		await store.EvictByTagAsync("user", ct);
		return Result(await repository.Subscribe(userId, contentId, transactionRefId));
	}
}