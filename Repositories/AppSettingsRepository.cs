namespace Utilities_aspnet.Repositories;

public interface IAppSettingsRepository {
	GenericResponse<EnumDto> ReadAppSettings();
	Task<GenericResponse<DashboardReadDto>> ReadDashboardData();
}

public class AppSettingsRepository : IAppSettingsRepository {
	private readonly IConfiguration _config;
	private readonly DbContext _dbContext;

	public AppSettingsRepository(IConfiguration config, DbContext dbContext) {
		_config = config;
		_dbContext = dbContext;
	}

	public GenericResponse<EnumDto> ReadAppSettings() {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		return new GenericResponse<EnumDto>(new EnumDto {
				DateTime = DateTime.Now,
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
				TagComments = EnumExtension.GetValues<TagComment>(),
				AppSettings = appSettings
			}
		);
	}

	public async Task<GenericResponse<DashboardReadDto>> ReadDashboardData() {
		DashboardReadDto dto = new() {
			Categories = await _dbContext.Set<CategoryEntity>().CountAsync(),
			Users = await _dbContext.Set<UserEntity>().CountAsync(),
			Products = await _dbContext.Set<ProductEntity>().CountAsync(),
			Orders = await _dbContext.Set<OrderEntity>().CountAsync(),
			Media = await _dbContext.Set<MediaEntity>().CountAsync(),
			Transactions = await _dbContext.Set<TransactionEntity>().CountAsync(),
		};
		return new GenericResponse<DashboardReadDto>(dto);
	}
}