namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : BaseApiController {
	private readonly IReportRepository _repository;

	public ReportController(IReportRepository repository) { _repository = repository; }

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Filter(ReportFilterDto parameters) { return Result(_repository.Read(parameters)); }

	[HttpGet]
	[OutputCache(PolicyName = "default")]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Read([FromQuery] ReportFilterDto parameters) {
		return Result(_repository.Read(parameters));
	}

	[HttpGet("{id:guid}")]
	[OutputCache(PolicyName = "default")]
	public async Task<ActionResult<GenericResponse<ReportEntity>>> ReadById(Guid id) { return Result(await _repository.ReadById(id)); }

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ReportEntity?>>> Create(ReportEntity parameters) { return Result(await _repository.Create(parameters)); }

	[HttpDelete]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) { return Result(await _repository.Delete(id)); }
}