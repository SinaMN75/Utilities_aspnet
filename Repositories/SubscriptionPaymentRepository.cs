namespace Utilities_aspnet.Repositories;

public interface ISubscriptionPaymentRepository {
	Task<GenericResponse<SubscriptionPaymentEntity?>> Create(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct);
	GenericResponse<IEnumerable<SubscriptionPaymentEntity>> Filter(SubscriptionPaymentFilter dto);
	Task<GenericResponse<SubscriptionPaymentEntity?>> Update(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class SubscriptionPaymentRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : ISubscriptionPaymentRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<SubscriptionPaymentEntity?>> Create(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct) {
		UserEntity? userUpgraded = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == dto.UserId, ct);
		PromotionEntity? promotionEntity = await dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.Id == dto.PromotionId, ct);
		if (userUpgraded == null && promotionEntity == null) return new GenericResponse<SubscriptionPaymentEntity?>(null, UtilitiesStatusCodes.Unhandled);
		if (userUpgraded != null && userUpgraded.Id != _userId)
			return new GenericResponse<SubscriptionPaymentEntity?>(null, UtilitiesStatusCodes.UserNotFound); // Is It Ok?

		SubscriptionPaymentEntity e = new() {
			PromotionId = dto.PromotionId,
			UserId = _userId,
			CreatedAt = DateTime.UtcNow,
			SubscriptionType = promotionEntity != null ? SubscriptionType.Promotion : SubscriptionType.UpgradeAccount,
			Tag = TagOrder.Pending
		};
		e.FillData(dto);

		await dbContext.AddAsync(e, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<SubscriptionPaymentEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		SubscriptionPaymentEntity? subscription = await dbContext.Set<SubscriptionPaymentEntity>().FirstOrDefaultAsync(f => f.Id == id, ct);
		if (subscription == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		dbContext.Remove(subscription);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}

	public GenericResponse<IEnumerable<SubscriptionPaymentEntity>> Filter(SubscriptionPaymentFilter dto) {
		IQueryable<SubscriptionPaymentEntity> q = dbContext.Set<SubscriptionPaymentEntity>().AsNoTracking();

		if (dto.ShowPromotion.IsTrue()) q = q.Include(x => x.Promotion);
		if (dto.ShowUser.IsTrue()) q = q.Include(x => x.User);
		if (dto.OrderByAmount.IsTrue()) q = q.OrderBy(x => x.Amount);
		if (dto.OrderBySubscriptionType.IsTrue()) q = q.OrderBy(x => x.SubscriptionType);

		return new GenericResponse<IEnumerable<SubscriptionPaymentEntity>>(q);
	}

	public async Task<GenericResponse<SubscriptionPaymentEntity?>> Update(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct) {
		SubscriptionPaymentEntity? subscription = await dbContext.Set<SubscriptionPaymentEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct);
		if (subscription == null) return new GenericResponse<SubscriptionPaymentEntity?>(null, UtilitiesStatusCodes.NotFound);
		subscription.FillData(dto);
		dbContext.Update(subscription);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<SubscriptionPaymentEntity?>(subscription);
	}
}

public static class SubscriptionPaymentEntityExtension {
	public static SubscriptionPaymentEntity FillData(this SubscriptionPaymentEntity entity, SubscriptionPaymentCreateUpdateDto dto) {
		entity.Tag = dto.Tag ?? entity.Tag;
		entity.SubscriptionType = dto.SubscriptionType ?? entity.SubscriptionType;
		entity.Amount = dto.Amount ?? entity.Amount;
		entity.UpdatedAt = DateTime.UtcNow;
		entity.Description = dto.Description ?? entity.Description;
		return entity;
	}
}