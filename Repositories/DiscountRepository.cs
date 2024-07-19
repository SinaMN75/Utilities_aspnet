namespace Utilities_aspnet.Repositories;

public interface IDiscountRepository {
	Task<GenericResponse<DiscountEntity>> Create(DiscountCreateDto dto);
	GenericResponse<IQueryable<DiscountEntity>> Filter(DiscountFilterDto dto);
	Task<GenericResponse<DiscountEntity?>> Update(DiscountUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class DiscountRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : IDiscountRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<DiscountEntity>> Create(DiscountCreateDto dto) {
		EntityEntry<DiscountEntity> i = await dbContext.AddAsync(new DiscountEntity {
			Title = dto.Title,
			Code = dto.Code,
			DiscountPrice = dto.DiscountPrice,
			NumberUses = dto.NumberUses,
			StartDate = dto.StartDate,
			EndDate = dto.EndDate,
			UserId = dto.UserId
		});
		await dbContext.SaveChangesAsync();
		return new GenericResponse<DiscountEntity>(i.Entity);
	}

	public GenericResponse<IQueryable<DiscountEntity>> Filter(DiscountFilterDto dto) {
		IQueryable<DiscountEntity> q = dbContext.Set<DiscountEntity>();

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(dto.Title!));
		if (dto.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code.Contains(dto.Code!));
		if (dto.NumberUses != null) q = q.Where(x => x.NumberUses == dto.NumberUses);
		if (dto.StartDate != null) q = q.Where(x => x.StartDate <= dto.StartDate);
		if (dto.EndDate != null) q = q.Where(x => x.EndDate >= dto.EndDate);

		int totalCount = q.Count();

		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).AsNoTracking();

		return new GenericResponse<IQueryable<DiscountEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<DiscountEntity?>> Update(DiscountUpdateDto dto) {
		DiscountEntity? e = await dbContext.Set<DiscountEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);

		if (e == null) return new GenericResponse<DiscountEntity?>(null, UtilitiesStatusCodes.NotFound);
		if (dto.Title is not null) e.Title = dto.Title;
		if (dto.NumberUses is not null) e.NumberUses = dto.NumberUses.Value;
		if (dto.Code is not null) e.Code = dto.Code;
		if (dto.StartDate is not null) e.StartDate = dto.StartDate.Value;
		if (dto.EndDate is not null) e.EndDate = dto.EndDate.Value;
		if (dto.Title is not null) e.Title = dto.Title;
		e.UpdatedAt = DateTime.UtcNow;
		await dbContext.SaveChangesAsync();
		return new GenericResponse<DiscountEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await dbContext.Set<DiscountEntity>().Where(x => x.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<DiscountEntity?>> ReadDiscountCode(string code) {
		DiscountEntity? discountEntity = await dbContext.Set<DiscountEntity>().FirstOrDefaultAsync(p => p.Code == code);
		if (discountEntity == null) return new GenericResponse<DiscountEntity?>(null, UtilitiesStatusCodes.NotFound);
		IQueryable<OrderEntity> orders = dbContext.Set<OrderEntity>()
			.Where(p => p.UserId == _userId && p.DiscountCode == code);
		return await orders.CountAsync() >= discountEntity.NumberUses
			? new GenericResponse<DiscountEntity?>(null, UtilitiesStatusCodes.MaximumLimitReached)
			: new GenericResponse<DiscountEntity?>(discountEntity);
	}
}