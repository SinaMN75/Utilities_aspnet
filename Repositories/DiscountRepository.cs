namespace Utilities_aspnet.Repositories;

public interface IDiscountRepository {
	Task<GenericResponse<DiscountEntity>> Create(DiscountCreateDto dto);
	Task<GenericResponse<IQueryable<DiscountEntity>>> Filter(DiscountFilterDto dto);
	Task<GenericResponse<DiscountEntity?>> Update(DiscountUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class DiscountRepository(DbContext dbContext) : IDiscountRepository {
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

	public async Task<GenericResponse<IQueryable<DiscountEntity>>> Filter(DiscountFilterDto dto) {
		IQueryable<DiscountEntity> q = dbContext.Set<DiscountEntity>().Select(
			x => new DiscountEntity {
				Id = x.Id,
				Title = x.Title,
				Code = x.Code,
				DiscountPrice = x.DiscountPrice,
				NumberUses = x.NumberUses,
				StartDate = x.StartDate,
				EndDate = x.EndDate,
				UserId = x.UserId
			});

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(dto.Title!));
		if (dto.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code.Contains(dto.Code!));
		if (dto.NumberUses != null) q = q.Where(x => x.NumberUses == dto.NumberUses);
		if (dto.StartDate != null) q = q.Where(x => x.StartDate <= dto.StartDate);
		if (dto.EndDate != null) q = q.Where(x => x.EndDate >= dto.EndDate);

		return await q.Paginate(dto);
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
}