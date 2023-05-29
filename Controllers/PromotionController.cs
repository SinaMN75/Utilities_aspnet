namespace Utilities_aspnet.Controllers;

public class PromotionController : BaseApiController {
	private readonly IPromotionRepository _repository;

	public PromotionController(IPromotionRepository repository) { _repository = repository; }

	[HttpPost("CreatePromotion")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> CreatePromotion(CreateUpdatePromotionDto dto) { return Result(await _repository.CreatePromotion(dto)); }

	[HttpGet("ReadPromotion/{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<PromotionDetail?>>> ReadPromotion(Guid id) { return Result(await _repository.ReadPromotion(id)); }
}