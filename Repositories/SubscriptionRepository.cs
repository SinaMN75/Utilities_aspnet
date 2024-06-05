namespace Utilities_aspnet.Repositories;

public interface ISubscriptionRepository {
	Task<GenericResponse<SubscriptionEntity?>> Create(SubscriptionCreateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<SubscriptionEntity>>> Filter(SubscriptionFilterDto dto);
	Task<GenericResponse<SubscriptionEntity?>> Update(SubscriptionUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid addressId, CancellationToken ct);
}

public class SubscriptionRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : ISubscriptionRepository {
	public async Task<GenericResponse<SubscriptionEntity?>> Create(SubscriptionCreateDto dto, CancellationToken ct) {
		EntityEntry<SubscriptionEntity> e = await dbContext.Set<SubscriptionEntity>().AddAsync(new SubscriptionEntity {
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			UserId = dto.UserId,
			Title = dto.Title,
			PaymentRefId = dto.PaymentRefId,
			ExpiresIn = dto.ExpiresIn,
			Tags = dto.Tags,
			JsonDetail = new SubscriptionJsonDetail { KeyValues = dto.KeyValues, StringList = dto.StringList }
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<SubscriptionEntity?>(e.Entity);
	}

	public async Task<GenericResponse<IQueryable<SubscriptionEntity>>> Filter(SubscriptionFilterDto dto) {
		IQueryable<SubscriptionEntity> q = dbContext.Set<SubscriptionEntity>().AsNoTracking().Select(x => new SubscriptionEntity {
			Id = x.Id,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			UserId = dto.UserId,
			Title = x.Title,
			PaymentRefId = x.PaymentRefId,
			ExpiresIn = x.ExpiresIn,
			Tags = dto.Tags,
			JsonDetail = x.JsonDetail
		});

		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(o => o.UserId == dto.UserId);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<SubscriptionEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<SubscriptionEntity?>> Update(SubscriptionUpdateDto dto, CancellationToken ct) {
		SubscriptionEntity e = (await dbContext.Set<SubscriptionEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		if (dto.PaymentRefId.IsNotNullOrEmpty()) e.PaymentRefId = dto.PaymentRefId!;
		if (dto.Title.IsNotNullOrEmpty()) e.Title = dto.Title!;
		if (dto.Tags.IsNotNullOrEmpty()) e.Tags = dto.Tags!;
		if (dto.KeyValues.IsNotNullOrEmpty()) e.JsonDetail.KeyValues = dto.KeyValues!;
		if (dto.StringList.IsNotNullOrEmpty()) e.JsonDetail.StringList = dto.StringList!;
		if (dto.ExpiresIn.HasValue) e.ExpiresIn = dto.ExpiresIn ?? DateTime.UtcNow;

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<SubscriptionEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid addressId, CancellationToken ct) {
		await dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}
}