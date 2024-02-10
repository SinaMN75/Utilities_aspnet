namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingsController(IAppSettingsRepository repository) : BaseApiController {
	[HttpGet]
	[OutputCache(Tags = ["appSetting"])]
	public ActionResult<GenericResponse<EnumDto>> Read() => repository.ReadAppSettings();

	[HttpGet("ReadDashboardData")]
	[OutputCache(Tags = ["appSetting"])]
	public async Task<ActionResult<GenericResponse<DashboardReadDto>>> ReadDashboardData() => await repository.ReadDashboardData();
	
	[HttpGet("ReadEverything")]
	[OutputCache(Tags = ["everything"])]
	public ActionResult<GenericResponse<EverythingReadDto>> ReadEverything(EverythingFilterDto dto) => repository.ReadEverything();
}