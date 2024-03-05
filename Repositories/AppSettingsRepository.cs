namespace Utilities_aspnet.Repositories;

public interface IAppSettingsRepository {
	GenericResponse<EnumDto> ReadAppSettings();
	Task<GenericResponse<DashboardReadDto>> ReadDashboardData();
	GenericResponse<EverythingReadDto> ReadEverything();
}

public class AppSettingsRepository(IConfiguration config, DbContext dbContext) : IAppSettingsRepository {
	public GenericResponse<EnumDto> ReadAppSettings() {
		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);
		return new GenericResponse<EnumDto>(new EnumDto {
				DateTime = DateTime.UtcNow,
				UtcDateTime = DateTime.UtcNow,
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
				TagPayment = EnumExtension.GetValues<TagPayment>(),
				TagReservationChair = EnumExtension.GetValues<TagReservationChair>(),
				TagUser = EnumExtension.GetValues<TagUser>(),
				AppSettings = appSettings
			}
		);
	}

	public async Task<GenericResponse<DashboardReadDto>> ReadDashboardData() =>
		new(new DashboardReadDto {
			Categories = await dbContext.Set<CategoryEntity>().AsNoTracking().CountAsync(),
			Users = await dbContext.Set<UserEntity>().AsNoTracking().CountAsync(),
			Products = await dbContext.Set<ProductEntity>().AsNoTracking().CountAsync(),
			Orders = await dbContext.Set<OrderEntity>().AsNoTracking().CountAsync(),
			Media = await dbContext.Set<MediaEntity>().AsNoTracking().CountAsync(),
			Transactions = await dbContext.Set<TransactionEntity>().AsNoTracking().CountAsync(),
			Reports = await dbContext.Set<ReportEntity>().AsNoTracking().CountAsync(),
			Address = await dbContext.Set<AddressEntity>().AsNoTracking().CountAsync(),
			ReleasedProducts = await dbContext.Set<ProductEntity>().AsNoTracking().Where(x => x.Tags.Contains(TagProduct.Released)).CountAsync(),
			InQueueProducts = await dbContext.Set<ProductEntity>().AsNoTracking().Where(x => x.Tags.Contains(TagProduct.InQueue)).CountAsync(),
			NotAcceptedProducts = await dbContext.Set<ProductEntity>().AsNoTracking().Where(x => x.Tags.Contains(TagProduct.NotAccepted)).CountAsync()
		});

	public GenericResponse<EverythingReadDto> ReadEverything() =>
		new(new EverythingReadDto {
				AppSettings = ReadAppSettings().Result,
				Categories = dbContext.Set<CategoryEntity>().AsNoTracking()
					.Include(x => x.Children)!.ThenInclude(x => x.Media)
					.Include(x => x.Media),
				Contents = dbContext.Set<ContentEntity>().AsNoTracking()
					.Include(x => x.Media),
				Products = dbContext.Set<ProductEntity>()
					.Include(x => x.Media!.Where(y => y.ParentId == null)).ThenInclude(x => x.Children)
					.Include(x => x.Categories)!.ThenInclude(x => x.Children)
					.Include(x => x.Comments)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			}
		);
}