namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingsController(IAppSettingsRepository repository) : BaseApiController {
	[HttpGet]
	[OutputCache(PolicyName = "appSetting")]
	public ActionResult<GenericResponse<EnumDto>> Read() => repository.ReadAppSettings();

	[HttpGet("ReadDashboardData")]
	[OutputCache(PolicyName = "appSetting")]
	public async Task<ActionResult<GenericResponse<DashboardReadDto>>> ReadDashboardData() => await repository.ReadDashboardData();
}