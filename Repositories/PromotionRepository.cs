namespace Utilities_aspnet.Repositories;

public interface IPromotionRepository {
	Task<GenericResponse<PromotionEntity?>> CreatePromotion(CreateUpdatePromotionDto dto);
	Task<GenericResponse> UserSeened(Guid id);
	Task<GenericResponse<PromotionDetail?>> GetPromotionTrackingInformation(Guid id);
	Task<GenericResponse<PromotionEntity?>> ReadById(Guid id);
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

	// noktei ke vojod dare dar in dto zamani ke dare az front ersal mishe faqat yeki az in property haye category ,userId, groupChatId , product bayad por bashe
	//2 ta por nabayad bashe
	public async Task<GenericResponse<PromotionEntity?>> CreatePromotion(CreateUpdatePromotionDto dto) {
		PromotionEntity? promotion = await _dbContext.Set<PromotionEntity>()
			.FirstOrDefaultAsync(f => f.ProductId == dto.ProductId || f.GroupChatId == dto.GroupChatId || f.UserPromotedId == dto.UserId ||
			                          f.CategoryId == dto.CategoryId);
		if (promotion is not null)
			return new GenericResponse<PromotionEntity?>(null, UtilitiesStatusCodes.BadRequest);

		promotion = new PromotionEntity {
			CreatedAt = DateTime.Now,
			ProductId = dto.ProductId,
			GroupChatId = dto.GroupChatId,
			CategoryId = dto.CategoryId,
			UserPromotedId = dto.UserId,
			UserId = _userId,
			DisplayType = dto.DisplayType,
			Gender = dto.Gender is not null ? string.Join(",", dto.Gender) : "",
			States = dto.States is not null ? string.Join(",", dto.States) : "",
			Skills = dto.Skills is not null ? string.Join(",", dto.Skills) : "",
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

		CategoryEntity? category = await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(f => f.Id == dto.CategoryId);
		if (category is not null) {
			category.JsonDetail.Boosted = DateTime.Now;
			_dbContext.Update(category);
		}

		UserEntity? userEntity = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == dto.UserId);
		if (userEntity is not null) {
			userEntity.JsonDetail.Boosted = DateTime.Now;
			_dbContext.Update(userEntity);
		}

		await _dbContext.SaveChangesAsync();
		return new GenericResponse<PromotionEntity?>(promotion);
	}

	public async Task<GenericResponse<PromotionDetail?>> GetPromotionTrackingInformation(Guid id) {
		//user Id ro ham guid gereftam vase inke dast nabaram to structure clean proje faqat kafie front az code payin estefade kone va userId ro tabdil be Guid kone befreste vasam
		//Uuid.parse('79700043-11eb-1101-80d6-510900000d10'); flutter
		PromotionEntity? promotion = await _dbContext.Set<PromotionEntity>()
			.FirstOrDefaultAsync(f => f.ProductId == id || f.GroupChatId == id || f.CategoryId == id || f.UserPromotedId == id.ToString());
		if (promotion is null) return new GenericResponse<PromotionDetail?>(null, UtilitiesStatusCodes.NotFound);

		TimeSpan timeDifference = DateTime.Now - promotion.CreatedAt;
		double hoursPassed = timeDifference.TotalHours;

		string[]? usersId = promotion.Users.IsNotNullOrEmpty() ? promotion.Users!.Split(",") : null;
		if (usersId is null) return new GenericResponse<PromotionDetail?>(null, UtilitiesStatusCodes.BadRequest);
		List<UserEntity> users = new();
		foreach (string? item in usersId) {
			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == item);
			if (user is null) continue;
			users.Add(user);
		}

		int userPerHour = (users.Count / hoursPassed * 100).ToInt();

		List<KeyValue> statePerUsers = users
			.GroupBy(u => u.State)
			.Select(g => new KeyValue { Key = g.Key!, Value = g.Count().ToString() })
			.ToList();

		List<KeyValue> skillPerUsers = users
			.SelectMany(u => u.Categories!)
			.GroupBy(c => new { c.Tags, c.Title })
			.Select(g => new KeyValue { Key = g.Key.Title ?? "", Value = g.Count().ToString() })
			.ToList();

		List<KeyValue> ageCategoryPerUsers = users
			.GroupBy(u => u.AgeCategory)
			.Select(g => new KeyValue { Key = g.Key.ToString()!, Value = g.Count().ToString() })
			.ToList();

		return new GenericResponse<PromotionDetail?>(new PromotionDetail {
			AgeCategoryPerUsers = ageCategoryPerUsers,
			SkillPerUsers = skillPerUsers,
			StatePerUsers = statePerUsers,
			TotalSeen = userPerHour
		});
	}

	public async Task<GenericResponse<PromotionEntity?>> ReadById(Guid id) {
		PromotionEntity? p = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.Id == id);
		return p is null ? new GenericResponse<PromotionEntity?>(null, UtilitiesStatusCodes.NotFound) : new GenericResponse<PromotionEntity?>(p);
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