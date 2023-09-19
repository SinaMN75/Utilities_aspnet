namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingsController : BaseApiController {
	private readonly IAppSettingsRepository _repository;

	public AppSettingsController(IAppSettingsRepository repository) { _repository = repository; }

	[HttpGet]
	public ActionResult<GenericResponse<EnumDto>> Read() => _repository.ReadAppSettings();

	[HttpGet("ReadDashboardData")]
	public async Task<ActionResult<GenericResponse<DashboardReadDto>>> ReadDashboardData() => await _repository.ReadDashboardData();
}