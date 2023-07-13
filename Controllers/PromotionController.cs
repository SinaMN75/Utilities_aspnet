namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController : BaseApiController {
	private readonly IPromotionRepository _repository;

	public PromotionController(IPromotionRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionEntity?>>> CreatePromotion(CreateUpdatePromotionDto dto) =>
		Result(await _repository.CreatePromotion(dto));

	[HttpGet("GetPromotionTrackingInformation/{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionDetail?>>> GetPromotionTrackingInformation(Guid id) =>
		Result(await _repository.GetPromotionTrackingInformation(id));

	[HttpGet("ReadById/{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionEntity?>>> ReadById(Guid id) => Result(await _repository.ReadById(id));
}