namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class AppSettingsController(IAppSettingsRepository repository) : BaseApiController {
	[HttpGet]
	public ActionResult<GenericResponse<EnumDto>> Read() => repository.ReadAppSettings();

	[HttpGet("ReadDashboardData")]
	public async Task<ActionResult<GenericResponse<DashboardReadDto>>> ReadDashboardData() => await repository.ReadDashboardData();

	[HttpGet("ReadEverything")]
	[OutputCache(Duration = 1)]
	public ActionResult<GenericResponse<EverythingReadDto>> ReadEverything(
		bool showProducts = true,
		bool showCategories = true,
		bool showContents = true
	) => repository.ReadEverything(showProducts, showCategories, showContents);
}