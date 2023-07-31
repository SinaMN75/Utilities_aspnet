namespace Utilities_aspnet.Repositories;

public interface IDiscountRepository {
	Task<GenericResponse<DiscountEntity>> Create(DiscountEntity dto);
	GenericResponse<IQueryable<DiscountEntity>> Filter(DiscountFilterDto dto);
	Task<GenericResponse<DiscountEntity?>> Update(DiscountEntity dto);
	Task<GenericResponse> Delete(Guid id);
	Task<GenericResponse<DiscountEntity?>> ReadDiscountCode(string code);
}

public class DiscountRepository : IDiscountRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public DiscountRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<DiscountEntity>> Create(DiscountEntity dto) {
		EntityEntry<DiscountEntity> i = await _dbContext.AddAsync(dto);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<DiscountEntity>(i.Entity);
	}

	public GenericResponse<IQueryable<DiscountEntity>> Filter(DiscountFilterDto dto) {
		IQueryable<DiscountEntity> q = _dbContext.Set<DiscountEntity>();

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

	public async Task<GenericResponse<DiscountEntity?>> Update(DiscountEntity dto) {
		DiscountEntity? e = await _dbContext.Set<DiscountEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);

		if (e == null) return new GenericResponse<DiscountEntity?>(null, UtilitiesStatusCodes.NotFound);
		e.Title = dto.Title;
		e.NumberUses = dto.NumberUses;
		e.Code = dto.Code;
		e.StartDate = dto.StartDate;
		e.EndDate = dto.EndDate;
		e.UpdatedAt = DateTime.Now;
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<DiscountEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await _dbContext.Set<DiscountEntity>().Where(x => x.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<DiscountEntity?>> ReadDiscountCode(string code) {
		DiscountEntity? discountEntity = await _dbContext.Set<DiscountEntity>().FirstOrDefaultAsync(p => p.Code == code);
		if (discountEntity == null) return new GenericResponse<DiscountEntity?>(null, UtilitiesStatusCodes.NotFound);
		IQueryable<OrderEntity> orders = _dbContext.Set<OrderEntity>()
			.Where(p => p.UserId == _userId && p.DiscountCode == code && p.Tags.Contains(TagOrder.Canceled));
		return await orders.CountAsync() >= discountEntity.NumberUses
			? new GenericResponse<DiscountEntity?>(null, UtilitiesStatusCodes.MaximumLimitReached)
			: new GenericResponse<DiscountEntity?>(discountEntity);
	}
}