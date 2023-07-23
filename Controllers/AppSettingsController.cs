namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingsController : BaseApiController {
	private readonly IConfiguration _config;

	public AppSettingsController(IConfiguration config) => _config = config;

	[HttpGet]
	public ActionResult<GenericResponse<EnumDto>> Read() {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		string hashedDateTime = Encryption
			.GetMd5HashData($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}SinaMN75").ToLower();
		return Result(new GenericResponse<EnumDto?>(new EnumDto {
			DateTime = DateTime.Now,
			HashedDateTime = hashedDateTime,
			FormFieldType = EnumExtension.GetValues<TagFormField>(),
			TransactionStatuses = EnumExtension.GetValues<TransactionStatus>(),
			TransactionType = EnumExtension.GetValues<TransactionType>(),
			UtilitiesStatusCodes = EnumExtension.GetValues<UtilitiesStatusCodes>(),
			Currency = EnumExtension.GetValues<Currency>(),
			SeenStatus = EnumExtension.GetValues<SeenStatus>(),
			Priority = EnumExtension.GetValues<Priority>(),
			ChatStatus = EnumExtension.GetValues<ChatStatus>(),
			Reaction = EnumExtension.GetValues<Reaction>(),
			GenderType = EnumExtension.GetValues<GenderType>(),
			AgeCategory = EnumExtension.GetValues<AgeCategory>(),
			ChatTypes = EnumExtension.GetValues<ChatType>(),
			PrivacyType = EnumExtension.GetValues<PrivacyType>(),
			Nationality = EnumExtension.GetValues<NationalityType>(),
			LegalAuthenticationType = EnumExtension.GetValues<LegalAuthenticationType>(),
			TagMedia = EnumExtension.GetValues<TagMedia>(),
			TagContent = EnumExtension.GetValues<TagContent>(),
			TagProduct = EnumExtension.GetValues<TagProduct>(),
			TagNotification = EnumExtension.GetValues<TagNotification>(),
			TagOrder = EnumExtension.GetValues<TagOrder>(),
			TagCategory = EnumExtension.GetValues<TagCategory>(),
			AppSettings = appSettings
		}));
	}
}