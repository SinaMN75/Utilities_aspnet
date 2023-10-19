namespace Utilities_aspnet.Repositories;

public interface ITransactionRepository {
	GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto);
	Task<GenericResponse<TransactionEntity>> Create(TransactionEntity dto, CancellationToken ct);
}

public class TransactionRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IOutputCacheStore outputCache) : ITransactionRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<TransactionEntity>> Create(TransactionEntity entity, CancellationToken ct) {
		entity.UserId ??= _userId;
		await dbContext.Set<TransactionEntity>().AddAsync(entity, ct);
		await dbContext.SaveChangesAsync(ct);
		await outputCache.EvictByTagAsync("transaction", ct);
		return new GenericResponse<TransactionEntity>(entity);
	}

	public GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto) {
		IQueryable<TransactionEntity> q = dbContext.Set<TransactionEntity>().Include(x => x.User).Include(x => x.Order).AsNoTracking();
		if (dto.RefId is not null) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.UserId is not null) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.Authority is not null) q = q.Where(x => x.Authority == dto.Authority);
		if (dto.Amount is not null) q = q.Where(x => x.Amount == dto.Amount);
		if (dto.ShebaNumber is not null) q = q.Where(x => x.ShebaNumber == dto.ShebaNumber);
		if (dto.OrderId is not null) q = q.Where(x => x.OrderId == dto.OrderId);
		if (dto.GatewayName is not null) q = q.Where(x => x.GatewayName == dto.GatewayName);
		if (dto.PaymentId is not null) q = q.Where(x => x.PaymentId == dto.PaymentId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		return new GenericResponse<IQueryable<TransactionEntity>>(q.AsNoTracking());
	}
}