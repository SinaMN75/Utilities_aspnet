namespace Utilities_aspnet.Repositories;

public interface IProductRepository {
	Task<GenericResponse<ProductEntity?>> Create(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto);
	Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct);
	Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
	Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto);
}

public class ProductRepository(
	DbContext dbContext,
	IHttpContextAccessor httpContextAccessor,
	IMediaRepository mediaRepository,
	INotificationRepository notificationRepository,
	IReportRepository reportRepository,
	ICommentRepository commentRepository
) : IProductRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<ProductEntity?>> Create(ProductCreateUpdateDto dto, CancellationToken ct) {
		DateTime yesterday = DateTime.Now.Subtract(TimeSpan.FromDays(1));
		int productsToday = await dbContext.Set<ProductEntity>()
			.Where(x => x.UserId == _userId && x.CreatedAt >= yesterday)
			.AsNoTracking()
			.CountAsync(ct);
		if (productsToday >= 30) return new GenericResponse<ProductEntity?>(null, UtilitiesStatusCodes.Overused);

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
		q = q.Where(x => x.ParentId == null);
		if (dto.Ids.IsNotNullOrEmpty()) q = q.Where(x => dto.Ids!.Contains(x.Id));
		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.Subtitle.IsNotNullOrEmpty()) q = q.Where(x => (x.Subtitle ?? "").Contains(dto.Subtitle!));
		if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => (x.Description ?? "").Contains(dto.Description!));
		if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State!.Contains(dto.State ?? ""));
		if (dto.Country.IsNotNullOrEmpty()) q = q.Where(x => x.Country!.Contains(dto.Country ?? ""));
		if (dto.City.IsNotNullOrEmpty()) q = q.Where(x => x.City!.Contains(dto.City ?? ""));
		if (dto.StateRegion.IsNotNullOrEmpty())
			q = q.Where(x => x.Country!.Contains(dto.StateRegion ?? "") || x.State!.Contains(dto.StateRegion ?? "") || x.City!.Contains(dto.StateRegion ?? ""));
		if (dto.StartPriceRange.HasValue) q = q.Where(x => x.Price >= dto.StartPriceRange);
		if (dto.Currency.HasValue) q = q.Where(x => x.Currency == dto.Currency);
		if (dto.EndPriceRange.HasValue) q = q.Where(x => x.Price <= dto.EndPriceRange);
		if (dto.StartDate.HasValue) q = q.Where(x => x.StartDate >= dto.StartDate);
		if (dto.EndDate.HasValue) q = q.Where(x => x.EndDate <= dto.EndDate);
		if (dto.Query is not null)
			q = q.Where(x => (x.Title ?? "").Contains(dto.Query!) || (x.Subtitle ?? "").Contains(dto.Query!) || (x.Description ?? "").Contains(dto.Query!));

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));
		if (dto.TagsInclude.IsNotNullOrEmpty()) q = q.Where(x => dto.TagsInclude!.Any(y => x.Tags.Contains(y)));
		if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.UserId));
		if (dto.FromDate is not null) q = q.Where(x => x.CreatedAt > dto.FromDate);
		if (dto.ShowExpired == false) q = q.Where(x => x.EndDate < DateTime.UtcNow || x.EndDate == null);

		if (dto.ShowChildren.IsTrue()) q = q.Include(i => i.Children)!.ThenInclude(x => x.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(x => x.Children);
		if (dto.ShowComments.IsTrue()) q = q.Include(i => i.Comments!.Where(x => x.Parent == null));
		if (dto.ShowCategoryMedia.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.Media);
		if (dto.ShowMedia.IsTrue()) q = q.Include(i => i.Media!.Where(j => j.ParentId == null)).ThenInclude(i => i.Children);

		if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
		if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
		if (dto.ShowCreator.IsTrue()) {
			q = q.Include(i => i.User).ThenInclude(x => x!.Media);
			q = q.Include(i => i.User).ThenInclude(x => x!.Categories);
		}

		if (dto.OrderByPriceAscending.IsTrue()) q = q.Where(w => w.Price.HasValue).OrderBy(o => o.Price);
		if (dto.OrderByPriceDescending.IsTrue()) q = q.Where(x => x.Price.HasValue).OrderByDescending(x => x.Price);
		if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedDateDescending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

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
			.Include(i => i.Parent).ThenInclude(i => i!.Categories)
			.Include(i => i.Parent).ThenInclude(i => i!.Media)
			.Include(i => i.Parent).ThenInclude(i => i!.Children)
			.Include(i => i.Comments)
			.Include(i => i.Parent).ThenInclude(i => i!.User).ThenInclude(i => i!.Media)
			.FirstOrDefaultAsync(i => i.Id == id, ct);
		if (i == null) return new GenericResponse<ProductEntity?>(null, UtilitiesStatusCodes.NotFound);

		if (!_userId.IsNotNullOrEmpty()) return new GenericResponse<ProductEntity?>(i);

		if (i.JsonDetail.VisitCounts.IsNullOrEmpty()) i.JsonDetail.VisitCounts = [new VisitCount { UserId = _userId ?? "", Count = 1 }];
		else {
			if (i.JsonDetail.VisitCounts!.Any(x => x.UserId == _userId)) {
				i.JsonDetail.VisitCounts!.First(x => x.UserId == _userId).Count += 1;
			}
			else {
				i.JsonDetail.VisitCounts!.Add(new VisitCount { UserId = _userId, Count = 1 });
			}
		}

		dbContext.Update(i);
		await dbContext.SaveChangesAsync(ct);

		return new GenericResponse<ProductEntity?>(i);
	}

	public async Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct) {
		ProductEntity entity = (await dbContext.Set<ProductEntity>()
			.Include(x => x.Categories)
			.Where(x => x.Id == dto.Id)
			.FirstOrDefaultAsync(ct))!;

		if (dto.Children.IsNotNullOrEmpty())
			foreach (ProductCreateUpdateDto childDto in dto.Children ?? []) {
				if (childDto.Id is null)
					await Create(childDto, ct);
				else {
					childDto.ParentId = dto.Id;
					await Update(childDto, ct);
				}
			}

		ProductEntity e = await entity.FillData(dto, dbContext);
		dbContext.Update(e);

		await dbContext.SaveChangesAsync(ct);

		return new GenericResponse<ProductEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ProductEntity i = (await dbContext.Set<ProductEntity>()
			.Include(x => x.Media)
			.Include(x => x.OrderDetail)
			.Include(x => x.Reports)
			.Include(x => x.Comments)
			.Include(x => x.Notifications)
			.Include(x => x.Children)!.ThenInclude(x => x.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.OrderDetail)
			.Include(x => x.Children)!.ThenInclude(x => x.Comments)
			.FirstOrDefaultAsync(x => x.Id == id, ct))!;
		foreach (CommentEntity comment in i.Comments ?? new List<CommentEntity>()) await commentRepository.Delete(comment.Id, ct);
		foreach (ReportEntity report in i.Reports ?? new List<ReportEntity>()) await reportRepository.Delete(report.Id);
		foreach (OrderDetailEntity orderDetail in i.OrderDetail ?? new List<OrderDetailEntity>()) dbContext.Remove(orderDetail);
		foreach (NotificationEntity notification in i.Notifications ?? new List<NotificationEntity>()) dbContext.Remove(notification);

		await mediaRepository.DeleteMedia(i.Media);
		foreach (ProductEntity product in i.Children ?? new List<ProductEntity>()) {
			await mediaRepository.DeleteMedia(product.Media);
			foreach (CommentEntity comment in product.Comments ?? new List<CommentEntity>()) await commentRepository.Delete(comment.Id, ct);
			foreach (OrderDetailEntity orderDetail in product.OrderDetail ?? new List<OrderDetailEntity>()) dbContext.Remove(orderDetail);
			dbContext.Remove(product);
		}

		dbContext.Remove(i);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto) {
		ProductEntity p = (await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == dto.ProductId))!;

		if (p.JsonDetail.UsersReactions.IsNullOrEmpty()) {
			p.JsonDetail.UsersReactions = [new UserReaction { Reaction = dto.Reaction, UserId = _userId! }];
		}
		else if (p.JsonDetail.UsersReactions!.Any(x => x.UserId == _userId)) {
			p.JsonDetail.UsersReactions!.First(x => x.UserId == _userId).Reaction = dto.Reaction;
		}
		else {
			p.JsonDetail.UsersReactions?.Add(new UserReaction {
				Reaction = dto.Reaction,
				UserId = _userId!
			});
		}

		if (p.UserId != _userId)
			await notificationRepository.Create(new NotificationCreateUpdateDto {
				UserId = p.UserId,
				Tags = [TagNotification.ReceivedReactionOnProduct],
				ProductId = p.Id,
				CreatorUserId = _userId
			});

		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}

public static class ProductEntityExtension {
	public static async Task<ProductEntity> FillData(this ProductEntity entity, ProductCreateUpdateDto dto, DbContext context) {
		entity.UpdatedAt = DateTime.UtcNow;
		if (dto.Title is not null) entity.Title = dto.Title;
		if (dto.Subtitle is not null) entity.Subtitle = dto.Subtitle;
		if (dto.State is not null) entity.State = dto.State;
		if (dto.City is not null) entity.City = dto.City;
		if (dto.Country is not null) entity.Country = dto.Country;
		if (dto.ExpireDate is not null) entity.ExpireDate = dto.ExpireDate;
		if (dto.Description is not null) entity.Description = dto.Description;
		if (dto.Price is not null) entity.Price = dto.Price;
		if (dto.Stock is not null) entity.Stock = dto.Stock;
		if (dto.StartDate is not null) entity.StartDate = dto.StartDate;
		if (dto.EndDate is not null) entity.EndDate = dto.EndDate;
		if (dto.Price1 is not null) entity.Price1 = dto.Price1;
		if (dto.Price2 is not null) entity.Price2 = dto.Price2;
		if (dto.Currency is not null) entity.Currency = dto.Currency;
		if (dto.Tags is not null) entity.Tags = dto.Tags;
		if (dto.Latitude is not null) entity.Latitude = dto.Latitude;
		if (dto.Longitude is not null) entity.Longitude = dto.Longitude;
		if (dto.Seats is not null) entity.JsonDetail.Seats = dto.Seats;
		if (dto.Color is not null) entity.JsonDetail.Color = dto.Color;
		if (dto.AdminMessage is not null) entity.JsonDetail.AdminMessage = dto.AdminMessage;
		if (dto.Author is not null) entity.JsonDetail.Author = dto.Author;
		if (dto.PhoneNumber is not null) entity.JsonDetail.PhoneNumber = dto.PhoneNumber;
		if (dto.Link is not null) entity.JsonDetail.Link = dto.Link;
		if (dto.Website is not null) entity.JsonDetail.Website = dto.Website;
		if (dto.Email is not null) entity.JsonDetail.Email = dto.Email;
		if (dto.Length is not null) entity.JsonDetail.Length = dto.Length;
		if (dto.Width is not null) entity.JsonDetail.Width = dto.Width;
		if (dto.Height is not null) entity.JsonDetail.Height = dto.Height;
		if (dto.Weight is not null) entity.JsonDetail.Weight = dto.Weight;
		if (dto.MinPrice is not null) entity.JsonDetail.MinPrice = dto.MinPrice;
		if (dto.MaxPrice is not null) entity.JsonDetail.MaxPrice = dto.MaxPrice;
		if (dto.Unit is not null) entity.JsonDetail.Unit = dto.Unit;
		if (dto.Address is not null) entity.JsonDetail.Address = dto.Address;
		if (dto.StartDate is not null) entity.JsonDetail.StartDate = dto.StartDate;
		if (dto.EndDate is not null) entity.JsonDetail.EndDate = dto.EndDate;
		if (dto.Details is not null) entity.JsonDetail.Details = dto.Details;
		if (dto.Type1 is not null) entity.JsonDetail.Type1 = dto.Type1;
		if (dto.Type2 is not null) entity.JsonDetail.Type2 = dto.Type2;
		if (dto.KeyValues is not null) entity.JsonDetail.KeyValues = dto.KeyValues;
		if (dto.ReservationTimes is not null) entity.JsonDetail.ReservationTimes = dto.ReservationTimes;
		if (dto.RelatedProducts is not null) entity.JsonDetail.RelatedProducts = dto.RelatedProducts;
		if (dto.ClubName is not null) entity.JsonDetail.ClubName = dto.ClubName;
		if (dto.Policies is not null) entity.JsonDetail.Policies = dto.Policies;
		if (dto.MaximumMembers is not null) entity.JsonDetail.MaximumMembers = dto.MaximumMembers;
		if (dto.PaymentRefId is not null) entity.JsonDetail.PaymentRefId = dto.PaymentRefId;

		if (dto.Categories.IsNotNull()) {
			List<CategoryEntity> listCategory = [];
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = await context.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (e != null) listCategory.Add(e);
			}

			entity.Categories = listCategory;
		}

		if (dto.ParentId is null) return entity;

		entity.ParentId = dto.ParentId ?? entity.ParentId;
		entity.Parent = context.Set<ProductEntity>().FirstOrDefault(f => f.Id == dto.ParentId);

		return entity;
	}
}