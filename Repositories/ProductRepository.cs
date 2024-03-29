namespace Utilities_aspnet.Repositories;

public interface IProductRepository {
	Task<GenericResponse<ProductEntity?>> Create(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto);
	Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct);
	Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
	Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto);
	GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id);
	GenericResponse<IQueryable<ReactionEntity>> FilterReaction(ReactionFilterDto dto);
	Task<GenericResponse<IQueryable<CustomersPaymentPerProduct>?>> GetMyCustomersPerProduct(Guid id);
}

public class ProductRepository(
	DbContext dbContext,
	IHttpContextAccessor httpContextAccessor,
	IMediaRepository mediaRepository,
	IUserRepository userRepository,
	IConfiguration config,
	IPromotionRepository promotionRepository,
	IFollowBookmarkRepository followBookMark,
	ICommentRepository commentRepository
)
	: IProductRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<ProductEntity?>> Create(ProductCreateUpdateDto dto, CancellationToken ct) {
		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);

		if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

		ProductEntity e = await new ProductEntity().FillData(dto, dbContext);
		e.UserId = _userId;
		e.CreatedAt = DateTime.UtcNow;

		EntityEntry<ProductEntity> i = await dbContext.Set<ProductEntity>().AddAsync(e, ct);
		await dbContext.SaveChangesAsync(ct);

		if (dto.Children is null) return new GenericResponse<ProductEntity?>(i.Entity);
		foreach (ProductCreateUpdateDto childDto in dto.Children) {
			if (childDto.Id is not null) await Update(childDto, ct);
			else {
				childDto.ParentId = i.Entity.Id;
				await Create(childDto, ct);
			}
		}

		return new GenericResponse<ProductEntity?>(i.Entity);
	}

	public async Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto) {
		IQueryable<ProductEntity> q = dbContext.Set<ProductEntity>().AsNoTracking();
		if (!dto.ShowExpired) q = q.Where(w => w.ExpireDate == null || w.ExpireDate >= DateTime.UtcNow);
		q = !dto.ShowWithChildren ? q.Where(x => x.ParentId == null) : q.Include(x => x.Parent);

		List<ProductEntity> postsThatMyFollowersSeen = [];
		if (dto.PostsThatMyFollowersSeen.IsTrue()) {
			GenericResponse<IQueryable<UserEntity>> myFollower = await followBookMark.GetFollowers(_userId!);
			if (myFollower.Result != null && myFollower.Result.Any()) {
				IQueryable<VisitProducts> productsThatMyFollowerSeen = dbContext.Set<VisitProducts>().Where(w => myFollower.Result.Any(a => a.Id == w.UserId));
				postsThatMyFollowersSeen = q.Where(x => productsThatMyFollowerSeen.Select(s => s.ProductId).Contains(x.Id)).ToList();
			}
		}

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.Subtitle.IsNotNullOrEmpty()) q = q.Where(x => (x.Subtitle ?? "").Contains(dto.Subtitle!));
		if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => (x.Description ?? "").Contains(dto.Description!));
		if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State == dto.State);
		if (dto.Region.IsNotNullOrEmpty()) q = q.Where(x => x.Region == dto.Region);
		if (dto.StartPriceRange.HasValue) q = q.Where(x => x.Price >= dto.StartPriceRange);
		if (dto.Currency.HasValue) q = q.Where(x => x.Currency == dto.Currency);
		if (dto.HasDiscount.IsTrue()) q = q.Where(x => x.DiscountPercent != null || x.DiscountPrice != null);
		if (dto.EndPriceRange.HasValue) q = q.Where(x => x.Price <= dto.EndPriceRange);
		if (dto.Query.IsNotNullOrEmpty())
			q = q.Where(x => (x.Title ?? "").Contains(dto.Query!) || (x.Subtitle ?? "").Contains(dto.Query!) || (x.Description ?? "").Contains(dto.Query!));

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));
		if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.UserId));

		if (dto.ShowPostOfPrivateUser != null && !dto.ShowPostOfPrivateUser.Value) {
			q = q.Include(i => i.User);
			q = q.Where(w => w.User != null);
		}

		if (dto.ShowChildren.IsTrue()) q = q.Include(i => i.Children)!.ThenInclude(x => x.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(x => x.Children);
		if (dto.ShowComments.IsTrue()) q = q.Include(i => i.Comments!.Where(x => x.Parent == null));
		if (dto.ShowCategoryMedia.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.Media);
		if (dto.ShowMedia.IsTrue()) q = q.Include(i => i.Media!.Where(j => j.ParentId == null)).ThenInclude(i => i.Children);
		if (dto.ShowVisitProducts.IsTrue()) q = q.Include(i => i.VisitProducts);
		if (dto.OrderByVotes.IsTrue()) q = q.OrderBy(x => x.VoteCount);
		if (dto.OrderByVotesDescending.IsTrue()) q = q.OrderByDescending(x => x.VoteCount);
		if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
		if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
		if (dto.OrderByAgeCategory.IsTrue()) q = q.OrderBy(o => o.AgeCategory);
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
			UserEntity user = (await userRepository.ReadByIdMinimal(_userId))!;
			if (dto.IsFollowing.IsTrue()) q = q.Where(i => user.FollowingUsers.Contains(i.UserId!));
			if (dto.IsBookmarked.IsTrue()) q = q.Where(i => user.BookmarkedProducts.Contains(i.Id.ToString()));
		}

		if (dto.Boosted) q = q.OrderByDescending(o => o.Boosted);

		q = q.Include(x => x.Parent).ThenInclude(x => x!.Categories);
		q = q.Include(x => x.Parent).ThenInclude(x => x!.Media);
		q = q.Include(x => x.Parent).ThenInclude(x => x!.User).ThenInclude(x => x!.Media);

		if (dto.OrderByPriceAscending.IsTrue()) q = q.Where(w => w.Price.HasValue).OrderBy(o => o.Price);
		if (dto.OrderByPriceDescending.IsTrue()) q = q.Where(x => x.Price.HasValue).OrderByDescending(x => x.Price);
		if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedDateDescending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		//Its Important to double check this condition , dont delete it <MohamadHosein>
		if (dto.PostsThatMyFollowersSeen.IsTrue()) {
			postsThatMyFollowersSeen.AddRange(q.ToList());
			q = postsThatMyFollowersSeen.AsQueryable();
		}

		if (dto.Shuffle1.IsTrue()) q = q.Shuffle();
		if (dto.Shuffle2.IsTrue()) {
			Random rand = new();
			q = q.OrderBy(_ => rand.Next()).ToList().AsQueryable();
		}

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		if (!dto.ShowCountOfComment.IsTrue())
			return new GenericResponse<IQueryable<ProductEntity>>(q) {
				TotalCount = totalCount,
				PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
				PageSize = dto.PageSize
			};
		{
			List<ProductEntity> tempQ = q.ToList();
			foreach (ProductEntity item in tempQ) {
				GenericResponse<IQueryable<CommentEntity>?> comments = commentRepository.ReadByProductId(item.Id);
				if (comments.Result != null) item.CommentsCount = comments.Result.Count(w => w.ParentId == null);
			}

			q = tempQ.AsQueryable();
		}

		return new GenericResponse<IQueryable<ProductEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct) {
		ProductEntity? i = await dbContext.Set<ProductEntity>()
			.Include(i => i.Media!.Where(j => j.ParentId == null)).ThenInclude(i => i.Children)
			.Include(i => i.Children)!.ThenInclude(x => x.Media)
			.Include(i => i.Categories)!.ThenInclude(x => x.Media)
			.Include(i => i.User).ThenInclude(x => x!.Media)
			.Include(i => i.User).ThenInclude(x => x!.Categories)
			.Include(i => i.VisitProducts)!.ThenInclude(i => i.User)
			.Include(i => i.ProductInsights)
			.Include(i => i.Parent).ThenInclude(i => i!.Categories)
			.Include(i => i.Parent).ThenInclude(i => i!.Media)
			.Include(i => i.Parent).ThenInclude(i => i!.Children)
			.Include(i => i.Comments)
			.Include(i => i.Parent).ThenInclude(i => i!.User).ThenInclude(i => i!.Media)
			.FirstOrDefaultAsync(i => i.Id == id, ct);
		if (i == null) return new GenericResponse<ProductEntity?>(null, UtilitiesStatusCodes.NotFound);

		if (i.JsonDetail.VisitCounts is null) i.JsonDetail.VisitCounts!.Add(new VisitCount { UserId = _userId ?? "", Count = 1 });
		else
			foreach (VisitCount j in i.JsonDetail.VisitCounts ?? []) {
				if (j.UserId == _userId) j.Count += 1;
				else {
					i.JsonDetail.VisitCounts!.Add(new VisitCount { UserId = _userId, Count = 0 });
				}
			}

		if (_userId.IsNotNullOrEmpty()) {
			UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId, ct);
			if (!user!.VisitedProducts.Contains(i.Id.ToString()))
				await userRepository.Update(new UserCreateUpdateDto { Id = _userId, VisitedProducts = user.VisitedProducts + "," + i.Id });

			VisitProducts? vp = await dbContext.Set<VisitProducts>().FirstOrDefaultAsync(a => a.UserId == user.Id && a.ProductId == i.Id, ct);
			if (vp is null) {
				VisitProducts visitProduct = new() { CreatedAt = DateTime.UtcNow, ProductId = i.Id, UserId = user.Id };
				await dbContext.Set<VisitProducts>().AddAsync(visitProduct, ct);
				if (string.IsNullOrEmpty(i.SeenUsers)) i.SeenUsers = user.Id;
				else i.SeenUsers = i.SeenUsers + "," + user.Id;
			}

			dbContext.Update(i);
			await dbContext.SaveChangesAsync(ct);
		}

		if (i.ProductInsights?.Any() != null) {
			List<IGrouping<ReactionEntity?, ProductInsight>> psGrouping = i.ProductInsights.GroupBy(g => g.Reaction).ToList();
			i.ProductInsights = null;
			List<ProductInsight> productInsights = [];
			foreach (IGrouping<ReactionEntity?, ProductInsight> item in psGrouping) {
				item.FirstOrDefault()!.Count = item.Count();
				productInsights.Add(item.FirstOrDefault()!);
			}

			i.ProductInsights = productInsights;
		}

		IQueryable<OrderEntity> completeOrder = dbContext.Set<OrderEntity>()
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Where(c => c.OrderDetails!.Any(w => w.ProductId == i.Id) && c.Tags.Contains(TagOrder.Complete)).AsNoTracking();
		i.SuccessfulPurchase = completeOrder.Count();

		i.Orders = completeOrder.OrderByDescending(x => x.TotalPrice);

		await promotionRepository.UserSeened(i.Id);
		return new GenericResponse<ProductEntity?>(i);
	}

	public async Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct) {
		ProductEntity entity = (await dbContext.Set<ProductEntity>()
			.Include(x => x.Categories)
			.Where(x => x.Id == dto.Id)
			.FirstOrDefaultAsync(ct))!;

		if (dto.Children is not null)
			foreach (ProductCreateUpdateDto childDto in dto.Children) {
				if (childDto.Id is null)
					await Create(childDto, ct);
				else {
					childDto.ParentId = dto.Id;
					await Update(childDto, ct);
				}
			}

		if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

		ProductEntity e = await entity.FillData(dto, dbContext);
		dbContext.Update(e);

		await dbContext.SaveChangesAsync(ct);

		return new GenericResponse<ProductEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ProductEntity i = (await dbContext.Set<ProductEntity>()
			.Include(x => x.Media)
			.Include(x => x.VisitProducts)
			.Include(x => x.OrderDetail)
			.Include(x => x.Comments)
			.Include(x => x.Bookmarks)!.ThenInclude(y => y.Media)
			.Include(x => x.Bookmarks)!.ThenInclude(y => y.Children)!.ThenInclude(z => z.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.VisitProducts)
			.Include(x => x.Children)!.ThenInclude(x => x.OrderDetail)
			.Include(x => x.Children)!.ThenInclude(x => x.Comments)
			.FirstOrDefaultAsync(x => x.Id == id, ct))!;
		foreach (CommentEntity comment in i.Comments ?? new List<CommentEntity>()) await commentRepository.Delete(comment.Id, ct);
		foreach (VisitProducts visitProduct in i.VisitProducts ?? new List<VisitProducts>()) dbContext.Remove(visitProduct);
		foreach (OrderDetailEntity orderDetail in i.OrderDetail ?? new List<OrderDetailEntity>()) dbContext.Remove(orderDetail);
		foreach (BookmarkEntity bookmark in i.Bookmarks ?? new List<BookmarkEntity>()) {
			foreach (BookmarkEntity bookmarkChild in bookmark.Children ?? new List<BookmarkEntity>()) {
				await mediaRepository.DeleteMedia(bookmarkChild.Media);
				dbContext.Remove(bookmarkChild);
			}
			await mediaRepository.DeleteMedia(bookmark.Media);
			dbContext.Remove(bookmark);
		}
		await mediaRepository.DeleteMedia(i.Media);
		foreach (ProductEntity product in i.Children ?? new List<ProductEntity>()) {
			await mediaRepository.DeleteMedia(product.Media);
			foreach (CommentEntity comment in product.Comments ?? new List<CommentEntity>()) await commentRepository.Delete(comment.Id, ct);
			foreach (VisitProducts visitProduct in product.VisitProducts ?? new List<VisitProducts>()) dbContext.Remove(visitProduct);
			foreach (OrderDetailEntity orderDetail in product.OrderDetail ?? new List<OrderDetailEntity>()) dbContext.Remove(orderDetail);
			dbContext.Remove(product);
		}

		IQueryable<ReactionEntity> reactions = dbContext.Set<ReactionEntity>().Where(x => x.ProductId == i.Id);
		if (reactions.Any()) dbContext.Set<ReactionEntity>().RemoveRange(reactions);
		dbContext.Remove(i);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto) {
		ReactionEntity? reaction = await dbContext.Set<ReactionEntity>().FirstOrDefaultAsync(f => f.UserId == _userId && f.ProductId == dto.ProductId);
		if (reaction?.Reaction == null) {
			reaction = new ReactionEntity {
				CreatedAt = DateTime.UtcNow,
				ProductId = dto.ProductId,
				Reaction = dto.Reaction,
				UserId = _userId
			};
			dbContext.Set<ReactionEntity>().Add(reaction);
		}
		//else if (reaction.Reaction.Value.HasFlag(dto.Reaction.Value))
		//{

		//}
		else {
			reaction.Reaction = dto.Reaction;
			reaction.UpdatedAt = DateTime.UtcNow;
			dbContext.Update(reaction);
		}

		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id) {
		IQueryable<ReactionEntity> reactions = dbContext.Set<ReactionEntity>()
			.Include(i => i.User)
			.Where(w => w.ProductId == id)
			.OrderBy(o => o.Reaction);
		return new GenericResponse<IQueryable<ReactionEntity>>(reactions);
	}

	public GenericResponse<IQueryable<ReactionEntity>> FilterReaction(ReactionFilterDto dto) {
		IQueryable<ReactionEntity> q = dbContext.Set<ReactionEntity>()
			.Include(i => i.User)
			.Where(w => w.ProductId == dto.ProductId);

		if (dto.Reaction.HasValue) q = q.Where(w => (int)w.Reaction!.Value == (int)dto.Reaction.Value);

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<ReactionEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<IQueryable<CustomersPaymentPerProduct>?>> GetMyCustomersPerProduct(Guid id) {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
		if (user is null) return new GenericResponse<IQueryable<CustomersPaymentPerProduct>?>(null, UtilitiesStatusCodes.UserNotFound);

		ProductEntity? product = await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == id);
		if (product is null) return new GenericResponse<IQueryable<CustomersPaymentPerProduct>?>(null, UtilitiesStatusCodes.NotFound);

		if (product.UserId != user.Id)
			return new GenericResponse<IQueryable<CustomersPaymentPerProduct>?>(null, UtilitiesStatusCodes.BadRequest, "This is not your product");

		IQueryable<OrderEntity> listOfOrders = dbContext.Set<OrderEntity>().Include(w => w.OrderDetails)
			.Where(w => w.ProductOwnerId == user.Id && w.PayDateTime != null && w.OrderDetails != null);
		if (!listOfOrders.Any())
			return new GenericResponse<IQueryable<CustomersPaymentPerProduct>?>(null, UtilitiesStatusCodes.Unhandled, "product owner hasn't any order yet");

		List<CustomersPaymentPerProduct> result = listOfOrders
			.Where(x => x.OrderDetails!.Any(y => y.ProductId == product.Id))
			.GroupBy(x => x.User)
			.Select(g => new CustomersPaymentPerProduct {
				Customer = g.Key,
				Payment = g.Sum(s => s.OrderDetails!.Where(w => w.ProductId == product.Id).Sum(so => so.FinalPrice))
			})
			.ToList();

		return new GenericResponse<IQueryable<CustomersPaymentPerProduct>?>(result.AsQueryable());
	}
}

public static class ProductEntityExtension {
	public static async Task<ProductEntity> FillData(this ProductEntity entity, ProductCreateUpdateDto dto, DbContext context) {
		entity.Title = dto.Title ?? entity.Title;
		entity.Subtitle = dto.Subtitle ?? entity.Subtitle;
		entity.State = dto.State ?? entity.State;
		entity.Region = dto.Region ?? entity.Region;
		entity.DiscountPrice = dto.DiscountPrice ?? entity.DiscountPrice;
		entity.DiscountPercent = dto.DiscountPercent ?? entity.DiscountPercent;
		entity.Description = dto.Description ?? entity.Description;
		entity.Price = dto.Price ?? entity.Price;
		entity.Stock = dto.Stock ?? entity.Stock;
		entity.CommentsCount = dto.CommentsCount ?? entity.CommentsCount;
		entity.Currency = dto.Currency ?? entity.Currency;
		entity.ExpireDate = dto.ExpireDate ?? entity.ExpireDate;
		entity.AgeCategory = dto.AgeCategory ?? entity.AgeCategory;
		entity.Boosted = dto.Boosted ?? entity.Boosted;
		entity.UpdatedAt = DateTime.UtcNow;
		entity.Tags = dto.Tags ?? entity.Tags;
		entity.Latitude = dto.Latitude ?? entity.Latitude;
		entity.Longitude = dto.Longitude ?? entity.Longitude;
		entity.JsonDetail = new ProductJsonDetail {
			Details = dto.Details ?? entity.JsonDetail.Details,
			Color = dto.Color ?? entity.JsonDetail.Color,
			AdminMessage = dto.AdminMessage ?? entity.JsonDetail.AdminMessage,
			Author = dto.Author ?? entity.JsonDetail.Author,
			PhoneNumber = dto.PhoneNumber ?? entity.JsonDetail.PhoneNumber,
			Link = dto.Link ?? entity.JsonDetail.Link,
			Website = dto.Website ?? entity.JsonDetail.Website,
			Email = dto.Email ?? entity.JsonDetail.Email,
			ResponseTime = dto.ResponseTime ?? entity.JsonDetail.ResponseTime,
			OnTimeDelivery = dto.OnTimeDelivery ?? entity.JsonDetail.OnTimeDelivery,
			Length = dto.Length ?? entity.JsonDetail.Length,
			Width = dto.Width ?? entity.JsonDetail.Width,
			Height = dto.Height ?? entity.JsonDetail.Height,
			Weight = dto.Weight ?? entity.JsonDetail.Weight,
			MinOrder = dto.MinOrder ?? entity.JsonDetail.MinOrder,
			MaxOrder = dto.MaxOrder ?? entity.JsonDetail.MaxOrder,
			MinPrice = dto.MinPrice ?? entity.JsonDetail.MinPrice,
			MaxPrice = dto.MaxPrice ?? entity.JsonDetail.MaxPrice,
			Unit = dto.Unit ?? entity.JsonDetail.Unit,
			Address = dto.Address ?? entity.JsonDetail.Address,
			StartDate = dto.StartDate ?? entity.JsonDetail.StartDate,
			EndDate = dto.EndDate ?? entity.JsonDetail.EndDate,
			ShippingCost = dto.ShippingCost ?? entity.JsonDetail.ShippingCost,
			ShippingTime = dto.ShippingTime ?? entity.JsonDetail.ShippingTime,
			KeyValue = dto.KeyValue ?? entity.JsonDetail.KeyValue,
			Type1 = dto.Type1 ?? entity.JsonDetail.Type1,
			Type2 = dto.Type2 ?? entity.JsonDetail.Type2,
			KeyValues = dto.KeyValues ?? entity.JsonDetail.KeyValues,
			Seats = dto.Seats ?? entity.JsonDetail.Seats,
			ReservationTimes = dto.ReservationTimes ?? entity.JsonDetail.ReservationTimes
		};

		if (dto.ScorePlus.HasValue) {
			if (entity.VoteCount == null) entity.VoteCount = 1;
			else entity.VoteCount += dto.ScorePlus;
		}

		if (dto.ScoreMinus.HasValue) {
			if (entity.VoteCount == null) entity.VoteCount = 1;
			else entity.VoteCount -= dto.ScoreMinus;
		}

		if (dto.Categories.IsNotNull()) {
			List<CategoryEntity> listCategory = [];
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = await context.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (e != null) listCategory.Add(e);
			}

			entity.Categories = listCategory;
		}

		if (dto.Teams.IsNotNull()) {
			string temp = dto.Teams!.Aggregate("", (current, userId) => current + userId + ",");
			entity.Teams = temp;
		}

		if (dto.RemoveTags.IsNotNullOrEmpty()) {
			dto.RemoveTags?.ForEach(item => entity.Tags.Remove(item));
		}

		if (dto.AddTags.IsNotNullOrEmpty()) {
			entity.Tags.AddRange(dto.AddTags!);
		}

		if (dto.ProductInsight is not null) {
			List<ProductInsight> productInsights = [];
			ProductInsightDto? pInsight = dto.ProductInsight;
			UserEntity? e = await context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == pInsight.UserId);
			if (e != null) {
				ProductInsight pI;
				ProductInsight? oldProductInsight = await context.Set<ProductInsight>().FirstOrDefaultAsync(f => f.UserId == e.Id && f.ProductId == entity.Id);
				if (oldProductInsight is not null && oldProductInsight.Reaction != pInsight.Reaction)
					pI = new ProductInsight {
						UserId = e.Id,
						Reaction = pInsight.Reaction,
						UpdatedAt = DateTime.UtcNow
					};
				else
					pI = new ProductInsight {
						UserId = e.Id,
						Reaction = pInsight.Reaction,
						CreatedAt = DateTime.UtcNow
					};
				await context.Set<ProductInsight>().AddAsync(pI);
				productInsights.Add(pI);
			}

			entity.ProductInsights = productInsights;
		}

		if (dto.ParentId is null) return entity;

		entity.ParentId = dto.ParentId ?? entity.ParentId;
		entity.Parent = context.Set<ProductEntity>().FirstOrDefault(f => f.Id == dto.ParentId);

		return entity;
	}
}