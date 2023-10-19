namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IReportRepository repository) : BaseApiController {
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Filter(ReportFilterDto parameters) => Result(repository.Read(parameters));

	[HttpGet]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Read([FromQuery] ReportFilterDto parameters) => Result(repository.Read(parameters));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<ReportEntity>>> ReadById(Guid id) => Result(await repository.ReadById(id));

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<ReportEntity?>>> Create(ReportCreateUpdateDto parameters) => Result(await repository.Create(parameters));

	[HttpDelete]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await repository.Delete(id));
}