namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController(IPromotionRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionEntity?>>> CreatePromotion(CreateUpdatePromotionDto dto) =>
		Result(await repository.CreatePromotion(dto));

	[HttpGet("GetPromotionTrackingInformation/{id}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionDetail?>>> GetPromotionTrackingInformation(Guid id) =>
		Result(await repository.GetPromotionTrackingInformation(id));

	[HttpGet("ReadById/{id}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<PromotionEntity?>>> ReadById(Guid id) => Result(await repository.ReadById(id));
}