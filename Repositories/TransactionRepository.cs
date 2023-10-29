namespace Utilities_aspnet.Repositories;

public interface ITransactionRepository {
	GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto);
	Task<GenericResponse<TransactionEntity>> Create(TransactionCreateDto dto, CancellationToken ct);
	Task<GenericResponse<TransactionEntity>> Update(TransactionUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class TransactionRepository(DbContext dbContext, IOutputCacheStore outputCache) : ITransactionRepository {

	public async Task<GenericResponse<TransactionEntity>> Create(TransactionCreateDto dto, CancellationToken ct) {
		TransactionEntity e = new() {
			Amount = dto.Amount,
			Descriptions = dto.Descriptions,
			RefId = dto.RefId,
			CardNumber = dto.CardNumber,
			Tags = dto.Tags,
			UserId = dto.UserId,
			OrderId = dto.OrderId,
			SubscriptionId = dto.SubscriptionId
		};
		await dbContext.Set<TransactionEntity>().AddAsync(e, ct);
		await dbContext.SaveChangesAsync(ct);
		await outputCache.EvictByTagAsync("transaction", ct);
		return new GenericResponse<TransactionEntity>(e);
	}

	public async Task<GenericResponse<TransactionEntity>> Update(TransactionUpdateDto dto, CancellationToken ct) {
		TransactionEntity e = (await dbContext.Set<TransactionEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct))!;
		if (dto.Amount.HasValue) e.Amount = dto.Amount;
		if (dto.RefId.IsNotNullOrEmpty()) e.RefId = dto.RefId;
		if (dto.Tags.IsNotNullOrEmpty()) e.Tags = dto.Tags!;
		if (dto.CardNumber.IsNotNullOrEmpty()) e.CardNumber = dto.CardNumber;
		if (dto.Descriptions.IsNotNullOrEmpty()) e.Descriptions = dto.Descriptions;
		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<TransactionEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) { 
		await dbContext.Set<TransactionEntity>().Where(x => x.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto) {
		IQueryable<TransactionEntity> q = dbContext.Set<TransactionEntity>().Include(x => x.User).Include(x => x.Order).AsNoTracking();
		if (dto.RefId is not null) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.UserId is not null) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.Amount is not null) q = q.Where(x => x.Amount == dto.Amount);
		if (dto.OrderId is not null) q = q.Where(x => x.OrderId == dto.OrderId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		return new GenericResponse<IQueryable<TransactionEntity>>(q.AsNoTracking());
	}
}