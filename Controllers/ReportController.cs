namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : BaseApiController {
	private readonly IReportRepository _repository;

	public ReportController(IReportRepository repository) => _repository = repository;

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Filter(ReportFilterDto parameters) => Result(_repository.Read(parameters));

	[HttpGet]
	[OutputCache(PolicyName = "default")]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Read([FromQuery] ReportFilterDto parameters) => Result(_repository.Read(parameters));

	[HttpGet("{id:guid}")]
	[OutputCache(PolicyName = "default")]
	public async Task<ActionResult<GenericResponse<ReportEntity>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<ReportEntity?>>> Create(ReportCreateUpdateDto parameters) => Result(await _repository.Create(parameters));

	[HttpDelete]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await _repository.Delete(id));
}