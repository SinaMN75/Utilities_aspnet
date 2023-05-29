namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class BlockController : BaseApiController {
	private readonly IUserRepository _repository;

	public BlockController(IUserRepository repository) { _repository = repository; }

	[HttpGet("ReadMine")]
	[OutputCache(PolicyName = "default")]
	public async Task<ActionResult<GenericResponse<IQueryable<UserEntity>>>> ReadMine() { return Result(await _repository.ReadMyBlockList()); }

	[HttpPost]
	public async Task<ActionResult<GenericResponse>> Create(string userId) { return Result(await _repository.ToggleBlock(userId)); }
}