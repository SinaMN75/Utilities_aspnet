namespace Utilities_aspnet.Repositories;

public interface IProductRepository {
	Task<GenericResponse<ProductEntity>> Create(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse<ProductEntity>> CreateWithFiles(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto);
	Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct);
	Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
	Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto);
	GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id);
}

public class ProductRepository : IProductRepository {
	private readonly IConfiguration _config;
	private readonly DbContext _dbContext;
	private readonly IMediaRepository _mediaRepository;
	private readonly IUserRepository _userRepository;
	private readonly IPromotionRepository _promotionRepository;
	private readonly string? _userId;

	public ProductRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor,
		IMediaRepository mediaRepository,
		IUserRepository userRepository,
		IConfiguration config,
		IPromotionRepository promotionRepository) {
		_dbContext = dbContext;
		_mediaRepository = mediaRepository;
		_userRepository = userRepository;
		_config = config;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
		_promotionRepository = promotionRepository;
	}

	public async Task<GenericResponse<ProductEntity>> Create(ProductCreateUpdateDto dto, CancellationToken ct) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		Tuple<bool, UtilitiesStatusCodes>? overUsedCheck =
			Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateProduct, null, dto.UseCase, appSettings.UsageRules!);
		if (overUsedCheck.Item1)
			return new GenericResponse<ProductEntity>(null, overUsedCheck.Item2);

		ProductEntity entity = new();

		if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

		ProductEntity e = await entity.FillData(dto, _dbContext);
		e.UserId = _userId;
		e.CreatedAt = DateTime.Now;

		EntityEntry<ProductEntity> i = await _dbContext.Set<ProductEntity>().AddAsync(e, ct);
		await _dbContext.SaveChangesAsync(ct);

		return new GenericResponse<ProductEntity>(i.Entity);
	}

	public async Task<GenericResponse<ProductEntity>> CreateWithFiles(ProductCreateUpdateDto dto, CancellationToken ct) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		Tuple<bool, UtilitiesStatusCodes>? overUsedCheck =
			Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateProduct, null, dto.UseCase, appSettings.UsageRules!);
		if (overUsedCheck.Item1)
			return new GenericResponse<ProductEntity>(null, overUsedCheck.Item2);

		ProductEntity entity = new();
		if (dto.Upload is not null) {
			foreach (UploadDto uploadDto in dto.Upload) await _mediaRepository.Upload(uploadDto);
		}

		if (dto.ProductInsight is not null)
			dto.ProductInsight.UserId = _userId;

		ProductEntity e = await entity.FillData(dto, _dbContext);
		e.UserId = _userId;
		e.CreatedAt = DateTime.Now;

		EntityEntry<ProductEntity> i = await _dbContext.Set<ProductEntity>().AddAsync(e, ct);
		await _dbContext.SaveChangesAsync(ct);

		return new GenericResponse<ProductEntity>(i.Entity);
	}

	public async Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto) {
		IQueryable<ProductEntity> q = _dbContext.Set<ProductEntity>();
		q = q.Where(x => x.DeletedAt == null);
		if (!dto.ShowExpired) q = q.Where(w => w.ExpireDate == null || w.ExpireDate >= DateTime.Now);

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.Subtitle.IsNotNullOrEmpty()) q = q.Where(x => (x.Subtitle ?? "").Contains(dto.Subtitle!));
		if (dto.Type.IsNotNullOrEmpty()) q = q.Where(x => (x.Type ?? "").Contains(dto.Type!));
		if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => (x.Description ?? "").Contains(dto.Description!));
		if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => x.UseCase.Contains(dto.UseCase));
		if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State == dto.State);
		if (dto.StartPriceRange.HasValue) q = q.Where(x => x.Price >= dto.StartPriceRange);
		if (dto.Currency.HasValue) q = q.Where(x => x.Currency == dto.Currency);
		if (dto.HasDiscount.IsTrue()) q = q.Where(x => x.DiscountPercent != null || x.DiscountPrice != null);
		if (dto.EndPriceRange.HasValue) q = q.Where(x => x.Price <= dto.EndPriceRange);
		if (dto.Enabled.HasValue) q = q.Where(x => x.Enabled == dto.Enabled);
		if (dto.Status.HasValue) q = q.Where(x => x.Status == dto.Status);
		if (dto.Query.IsNotNullOrEmpty())
			q = q.Where(x => (x.Title ?? "").Contains(dto.Query!) || (x.Subtitle ?? "").Contains(dto.Query!) || (x.Description ?? "").Contains(dto.Query!));

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));
		if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.UserId));

		if (dto.ShowCategories.IsTrue()) q = q.Include(i => i.Categories);
		if (dto.ShowCategoriesFormFields.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.FormFields);
		if (dto.ShowCategoryMedia.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.Media);
		if (dto.ShowForms.IsTrue()) q = q.Include(i => i.Forms);
		if (dto.ShowFormFields.IsTrue()) q = q.Include(i => i.Forms)!.ThenInclude(i => i.FormField);
		if (dto.ShowMedia.IsTrue()) q = q.Include(i => i.Media);
		if (dto.ShowVisitProducts.IsTrue()) q = q.Include(i => i.VisitProducts);
		if (dto.OrderByVotes.IsTrue()) q = q.OrderBy(x => x.VoteCount);
		if (dto.OrderByVotesDecending.IsTrue()) q = q.OrderByDescending(x => x.VoteCount);
		if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
		if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
		if (dto.OrderByPriceAccending.IsTrue()) q = q.OrderBy(x => x.Price);
		if (dto.OrderByPriceDecending.IsTrue()) q = q.OrderByDescending(x => x.Price);
		if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByCreaedDateDecending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByAgeCategory.IsTrue()) q = q.OrderBy(o => o.AgeCategory);
		if (dto.OrderByMostUsedHashtag.IsTrue()) q = q.OrderBy(o => o.Categories.Count(c => c.UseCase.ToLower() == "tag"));

		if (dto.OrderByCategory.IsTrue())
			q = q.AsEnumerable().OrderBy(o => o.Categories != null && o.Categories.Any()
				                             ? o.Categories.OrderBy(op => op.Title)
					                             .Select(s => s.Title)
					                             .FirstOrDefault() ?? string.Empty
				                             : string.Empty).AsQueryable();
		if (dto.ShowCreator.IsTrue()) {
			q = q.Include(i => i.User).ThenInclude(x => x!.Media);
			q = q.Include(i => i.User).ThenInclude(x => x!.Categories);
		}

		if (_userId.IsNotNullOrEmpty()) {
			UserEntity user = (await _userRepository.ReadByIdMinimal(_userId))!;
			if (dto.IsFollowing.IsTrue()) q = q.Where(i => user.FollowingUsers.Contains(i.UserId!));
			if (dto.IsBookmarked.IsTrue()) q = q.Where(i => user.BookmarkedProducts.Contains(i.Id.ToString()));
		}

		if (dto.Boosted) q = q.OrderByDescending(o => o.Boosted);

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<ProductEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct) {
		ProductEntity? i = await _dbContext.Set<ProductEntity>()
			.Include(i => i.Media)
			.Include(i => i.Categories)!.ThenInclude(x => x.Media)
			.Include(i => i.User).ThenInclude(x => x.Media)
			.Include(i => i.User).ThenInclude(x => x.Categories)
			.Include(i => i.Forms)!.ThenInclude(x => x.FormField)
			.Include(i => i.VisitProducts)!.ThenInclude(i => i.User)
			.Include(i => i.ProductInsights)
			.FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null, ct);
		if (i == null) return new GenericResponse<ProductEntity?>(null, UtilitiesStatusCodes.NotFound);

		if (_userId.IsNotNullOrEmpty()) {
			UserEntity? user = await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == _userId, ct);
			if (!user.VisitedProducts.Contains(i.Id.ToString()))
				await _userRepository.Update(new UserCreateUpdateDto {
					Id = _userId,
					VisitedProducts = user.VisitedProducts + "," + i.Id
				});

			VisitProducts? vp = await _dbContext.Set<VisitProducts>().FirstOrDefaultAsync(a => a.UserId == user.Id && a.ProductId == i.Id, ct);
			if (vp is null) {
				VisitProducts visitProduct = new() {
					CreatedAt = DateTime.Now,
					ProductId = i.Id,
					UserId = user.Id,
				};
				await _dbContext.Set<VisitProducts>().AddAsync(visitProduct, ct);
				if (string.IsNullOrEmpty(i.SeenUsers)) i.SeenUsers = user.Id;
				else i.SeenUsers = i.SeenUsers + "," + user.Id;
			}
			_dbContext.Update(i);
			await _dbContext.SaveChangesAsync(ct);
		}

		if (i.ProductInsights?.Any() != null) {
			List<IGrouping<ReactionEntity?, ProductInsight>> psGrouping = i.ProductInsights.GroupBy(g => g.Reaction).ToList();
			i.ProductInsights = null;
			List<ProductInsight> productInsights = new();
			foreach (IGrouping<ReactionEntity?, ProductInsight> item in psGrouping) {
				item.FirstOrDefault().Count = item.Count();
				productInsights.Add(item.FirstOrDefault());
			}
			i.ProductInsights = productInsights;
		}

		IQueryable<OrderEntity>? completeOrder = _dbContext.Set<OrderEntity>()
			.Where(c => c.OrderDetails != null && c.OrderDetails.Any(w => w.ProductId.HasValue && w.ProductId.Value == i.Id) &&
			            c.Status == OrderStatuses.Complete);
		string? displayOrderComplete = Utils.DisplayCountOfCompleteOrder(completeOrder.Count());
		i.SuccessfulPurchase = displayOrderComplete;

		bool isUserBuyIt = completeOrder.Any(a => a.UserId == _userId);
		if (isUserBuyIt) i.Media?.Select(s => s.MediaJsonDetail.Link == "");

		await _promotionRepository.UserSeened(i.Id);

		return new GenericResponse<ProductEntity?>(i);
	}

	public async Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct) {
		ProductEntity? entity = await _dbContext.Set<ProductEntity>()
			.Include(x => x.Categories)
			.Where(x => x.Id == dto.Id)
			.FirstOrDefaultAsync(ct);

		if (entity == null) return new GenericResponse<ProductEntity>(new ProductEntity());

		if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

		ProductEntity e = await entity.FillData(dto, _dbContext);
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync(ct);

		return new GenericResponse<ProductEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ProductEntity? i = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
		if (i != null) {
			i.DeletedAt = DateTime.Now;
			_dbContext.Update(i);
			await _dbContext.SaveChangesAsync(ct);
			return new GenericResponse(message: "Deleted");
		}
		return new GenericResponse(UtilitiesStatusCodes.NotFound, "Notfound");
	}

	public async Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto) {
		ReactionEntity? reaction = await _dbContext.Set<ReactionEntity>().FirstOrDefaultAsync(f => f.UserId == _userId && f.ProductId == dto.ProductId);
		if (reaction is not null && reaction.Reaction != dto.Reaction) {
			reaction.Reaction = dto.Reaction;
			reaction.UpdatedAt = DateTime.UtcNow;
			_dbContext.Update(reaction);
		}
		else {
			reaction = new() {
				CreatedAt = DateTime.UtcNow,
				ProductId = dto.ProductId,
				Reaction = dto.Reaction,
				UserId = _userId,
			};
			await _dbContext.Set<ReactionEntity>().AddAsync(reaction);
		}
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id) {
		IQueryable<ReactionEntity> reactions = _dbContext.Set<ReactionEntity>()
			.Include(i => i.User)
			.Where(w => w.ProductId == id)
			.OrderBy(o => o.Reaction);

		return new GenericResponse<IQueryable<ReactionEntity>>(reactions);
	}
}

public static class ProductEntityExtension {
	public static async Task<ProductEntity> FillData(this ProductEntity entity, ProductCreateUpdateDto dto, DbContext context) {
		entity.Title = dto.Title ?? entity.Title;
		entity.Subtitle = dto.Subtitle ?? entity.Subtitle;
		entity.Type = dto.Type ?? entity.Type;
		entity.State = dto.State ?? entity.State;
		entity.DiscountPrice = dto.DiscountPrice ?? entity.DiscountPrice;
		entity.DiscountPercent = dto.DiscountPercent ?? entity.DiscountPercent;
		entity.Description = dto.Description ?? entity.Description;
		entity.UseCase = dto.UseCase ?? entity.UseCase;
		entity.Price = dto.Price ?? entity.Price;
		entity.Enabled = dto.Enabled ?? entity.Enabled;
		entity.ProductJsonDetail.Details = dto.Details ?? entity.ProductJsonDetail.Details;
		entity.ProductJsonDetail.Author = dto.Author ?? entity.ProductJsonDetail.Author;
		entity.ProductJsonDetail.PhoneNumber = dto.PhoneNumber ?? entity.ProductJsonDetail.PhoneNumber;
		entity.ProductJsonDetail.Link = dto.Link ?? entity.ProductJsonDetail.Link;
		entity.ProductJsonDetail.Website = dto.Website ?? entity.ProductJsonDetail.Website;
		entity.ProductJsonDetail.Email = dto.Email ?? entity.ProductJsonDetail.Email;
		entity.ProductJsonDetail.Longitude = dto.Longitude ?? entity.ProductJsonDetail.Longitude;
		entity.ProductJsonDetail.Latitude = dto.Latitude ?? entity.ProductJsonDetail.Latitude;
		entity.ProductJsonDetail.ResponseTime = dto.ResponseTime ?? entity.ProductJsonDetail.ResponseTime;
		entity.ProductJsonDetail.OnTimeDelivery = dto.OnTimeDelivery ?? entity.ProductJsonDetail.OnTimeDelivery;
		entity.ProductJsonDetail.Length = dto.Length ?? entity.ProductJsonDetail.Length;
		entity.ProductJsonDetail.Width = dto.Width ?? entity.ProductJsonDetail.Width;
		entity.ProductJsonDetail.Height = dto.Height ?? entity.ProductJsonDetail.Height;
		entity.ProductJsonDetail.Weight = dto.Weight ?? entity.ProductJsonDetail.Weight;
		entity.ProductJsonDetail.MinOrder = dto.MinOrder ?? entity.ProductJsonDetail.MinOrder;
		entity.ProductJsonDetail.MaxOrder = dto.MaxOrder ?? entity.ProductJsonDetail.MaxOrder;
		entity.ProductJsonDetail.MinPrice = dto.MinPrice ?? entity.ProductJsonDetail.MinPrice;
		entity.ProductJsonDetail.MaxPrice = dto.MaxPrice ?? entity.ProductJsonDetail.MaxPrice;
		entity.ProductJsonDetail.Unit = dto.Unit ?? entity.ProductJsonDetail.Unit;
		entity.ProductJsonDetail.Address = dto.Address ?? entity.ProductJsonDetail.Address;
		entity.ProductJsonDetail.StartDate = dto.StartDate ?? entity.ProductJsonDetail.StartDate;
		entity.ProductJsonDetail.EndDate = dto.EndDate ?? entity.ProductJsonDetail.EndDate;
		entity.ProductJsonDetail.ShippingCost = dto.ShippingCost ?? entity.ProductJsonDetail.ShippingCost;
		entity.ProductJsonDetail.ShippingTime = dto.ShippingTime ?? entity.ProductJsonDetail.ShippingTime;
		entity.ProductJsonDetail.KeyValue = dto.KeyValue ?? entity.ProductJsonDetail.KeyValue;
		entity.Stock = dto.Stock ?? entity.Stock;
		entity.Status = dto.Status ?? entity.Status;
		entity.CommentsCount = dto.CommentsCount ?? entity.CommentsCount;
		entity.Currency = dto.Currency ?? entity.Currency;
		entity.ExpireDate = dto.ExpireDate ?? entity.ExpireDate;
		entity.AgeCategory = dto.AgeCategory ?? entity.AgeCategory;
		entity.ProductState = dto.ProductState ?? entity.ProductState;
		entity.Boosted = dto.Boosted ?? entity.Boosted;
		entity.UpdatedAt = DateTime.Now;

		if (dto.ScorePlus.HasValue) {
			if (entity.VoteCount == null) entity.VoteCount = 1;
			else entity.VoteCount += dto.ScorePlus;
		}

		if (dto.ScoreMinus.HasValue) {
			if (entity.VoteCount == null) entity.VoteCount = 1;
			else entity.VoteCount -= dto.ScoreMinus;
		}

		if (dto.Categories.IsNotNull()) {
			List<CategoryEntity> listCategory = new();
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = await context.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (e != null) listCategory.Add(e);
			}
			entity.Categories = listCategory;
		}

		if (dto.Teams.IsNotNull()) {
			string temp = "";
			foreach (string userId in dto.Teams!) temp += userId + ",";
			entity.Teams = temp;
		}

		if (dto.ProductInsight is not null) {
			List<ProductInsight> productInsights = new();
			ProductInsightDto? pInsight = dto.ProductInsight;
			UserEntity? e = await context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == pInsight.UserId);
			if (e != null) {
				ProductInsight pI;
				ProductInsight? oldProductInsight = await context.Set<ProductInsight>().FirstOrDefaultAsync(f => f.UserId == e.Id && f.ProductId == entity.Id);
				if (oldProductInsight is not null && oldProductInsight.Reaction != pInsight.Reaction) {
					pI = new ProductInsight {
						UserId = e.Id,
						Reaction = pInsight.Reaction,
						UpdatedAt = DateTime.Now
					};
				}
				else {
					pI = new ProductInsight {
						UserId = e.Id,
						Reaction = pInsight.Reaction,
						CreatedAt = DateTime.Now
					};
				}
				await context.Set<ProductInsight>().AddAsync(pI);
				productInsights.Add(pI);
			}
			entity.ProductInsights = productInsights;
		}

		if (dto.ParentId is not null) {
			entity.ParentId = dto.ParentId ?? entity.ParentId;
			entity.Product = context.Set<ProductEntity>().FirstOrDefault(f => f.Id == dto.ParentId);
		}

		return entity;
	}
}