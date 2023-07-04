namespace Utilities_aspnet.Repositories;

public interface IPromotionRepository {
	Task<GenericResponse> CreatePromotion(CreateUpdatePromotionDto dto);
	Task<GenericResponse> UserSeened(Guid id);
	Task<GenericResponse<PromotionDetail?>> ReadPromotion(Guid id);
}

public class PromotionRepository : IPromotionRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public PromotionRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse> CreatePromotion(CreateUpdatePromotionDto dto) {
		PromotionEntity? promotion = await _dbContext.Set<PromotionEntity>()
			.FirstOrDefaultAsync(f => f.ProductId == dto.ProductId || f.GroupChatId == dto.GroupChatId);
		if (promotion is not null)
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		promotion = new PromotionEntity {
			CreatedAt = DateTime.Now,
			ProductId = dto.ProductId,
			UserId = _userId,
			DisplayType = dto.DisplayType,
			Gender = dto.Gender is not null ? string.Join(",", dto.Gender) : "",
			States = dto.States is not null ? string.Join(",", dto.States) : "",
			Skills = dto.Gender is not null ? string.Join(",", dto.Skills) : "",
			AgeCategories = dto.AgeCategories is not null ? string.Join(",", dto.AgeCategories) : ""
		};
		await _dbContext.Set<PromotionEntity>().AddAsync(promotion);

		ProductEntity? product = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == dto.ProductId);
		if (product is not null) {
			product.Boosted = DateTime.Now;
			_dbContext.Update(product);
		}

		GroupChatEntity? groupChat = await _dbContext.Set<GroupChatEntity>().FirstOrDefaultAsync(f => f.Id == dto.GroupChatId);
		if (groupChat is not null) {
			groupChat.JsonDetail.Boosted = DateTime.Now;
			_dbContext.Update(groupChat);
		}
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<PromotionDetail?>> ReadPromotion(Guid id) {
		PromotionEntity? promotion = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.ProductId == id || f.GroupChatId == id);
		if (promotion is null) return new GenericResponse<PromotionDetail?>(null, UtilitiesStatusCodes.NotFound);

		TimeSpan timeDifference = DateTime.Now - promotion.CreatedAt!.Value;
		double hoursPassed = timeDifference.TotalHours;

		string[]? usersId = promotion?.Users.IsNotNullOrEmpty() ?? false ? promotion.Users.Split(",") : null;
		if (usersId is null) return new GenericResponse<PromotionDetail?>(null, UtilitiesStatusCodes.BadRequest);
		List<UserEntity> users = new();
		foreach (string? item in usersId) {
			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == item);
			if (item is null || user is null) continue;
			users.Add(user);
		}

		int userPerHour = (users.Count() / hoursPassed * 100).ToInt();

		List<StatePerUser> statePerUsers = users
			.GroupBy(u => u.State)
			.Select(g => new StatePerUser { State = g.Key, UserCount = g.Count() })
			.ToList();

		List<SkillPerUser> skillPerUsers = users
			.SelectMany(u => u.Categories)
			.GroupBy(c => new { c.UseCase, c.Title })
			.Select(g => new SkillPerUser { Skill = g?.Key.Title ?? "", UserCount = g?.Count() ?? 0 })
			.ToList();

		List<AgeCategoryPerUser> ageCategoryPerUsers = users
			.GroupBy(u => u.AgeCategory)
			.Select(g => new AgeCategoryPerUser { AgeCategory = ((int) g.Key).ToString(), UserCount = g.Count() })
			.ToList();

		return new GenericResponse<PromotionDetail?>(new PromotionDetail {
			AgeCategoryPerUsers = ageCategoryPerUsers,
			SkillPerUsers = skillPerUsers,
			StatePerUsers = statePerUsers,
			TotalSeen = userPerHour
		});
	}

	public async Task<GenericResponse> UserSeened(Guid id) {
		PromotionEntity? promotion = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.ProductId == id || f.GroupChatId == id);
		if (promotion is null)
			return new GenericResponse(UtilitiesStatusCodes.NotFound);
		promotion.Users = promotion.Users.AddCommaSeperatorUsers(_userId);
		_dbContext.Update(promotion);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}