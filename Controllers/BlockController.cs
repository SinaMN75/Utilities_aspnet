namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class BlockController : BaseApiController {
	private readonly IBlockRepository _repository;

	public BlockController(IBlockRepository repository) => _repository = repository;

	[HttpGet("ReadMine")]
	[ResponseCache(Duration = 20, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new [] {"impactlevel", "pii"})]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserReadDto>>>> ReadMine() {
		GenericResponse<IEnumerable<UserReadDto>> result = await _repository.ReadMine();
		return Result(result);
	}

	[HttpPost]
	[ResponseCache(Duration = 20, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new [] {"impactlevel", "pii"})]
	public async Task<ActionResult<GenericResponse>> Create(string userId) {
		GenericResponse result = await _repository.ToggleBlock(userId);
		return Result(result);
	}
}