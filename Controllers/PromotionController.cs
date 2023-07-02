namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController : BaseApiController {
	private readonly IPromotionRepository _repository;

	public PromotionController(IPromotionRepository repository) => _repository = repository;

	[HttpPost()]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> CreatePromotion(CreateUpdatePromotionDto dto) => Result(await _repository.CreatePromotion(dto));

	[HttpGet("ReadPromotion/{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionDetail?>>> ReadPromotion(Guid id) => Result(await _repository.ReadPromotion(id));
}