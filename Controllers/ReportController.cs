namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController(IReportRepository repository) : BaseApiController {
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IEnumerable<ReportEntity>>> Filter() => Result(repository.Filter());

	[HttpGet("{id}")]
	public async Task<ActionResult<GenericResponse<ReportEntity>>> ReadById(Guid id) => Result(await repository.ReadById(id));

	[HttpPost]
	public async Task<ActionResult<GenericResponse<ReportEntity?>>> Create(ReportCreateUpdateDto dto) => Result(await repository.Create(dto));

	[HttpDelete]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await repository.Delete(id));
}