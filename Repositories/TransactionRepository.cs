namespace Utilities_aspnet.Repositories;

public interface ITransactionRepository {
	GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto);
	GenericResponse<IQueryable<TransactionEntity>> ReadMine();
	Task<GenericResponse<TransactionEntity>> Create(TransactionEntity dto, CancellationToken ct);
}

public class TransactionRepository : ITransactionRepository {
	private readonly DbContext _dbContext;
	private readonly IOutputCacheStore _outputCache;
	private readonly string? _userId;

	public TransactionRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IOutputCacheStore outputCache) {
		_dbContext = dbContext;
		_outputCache = outputCache;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<TransactionEntity>> Create(TransactionEntity entity, CancellationToken ct) {
		entity.UserId ??= _userId;
		await _dbContext.Set<TransactionEntity>().AddAsync(entity, ct);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("transaction", ct);
		return new GenericResponse<TransactionEntity>(entity);
	}
	
	public GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto) {
		IQueryable<TransactionEntity> q = _dbContext.Set<TransactionEntity>().Include(x => x.User).Include(x => x.Order).AsNoTracking();
		if (dto.RefId is not null) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.StatusId is not null) q = q.Where(x => x.StatusId == dto.StatusId);
		if (dto.UserId is not null) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.Authority is not null) q = q.Where(x => x.Authority == dto.Authority);
		if (dto.Amount is not null) q = q.Where(x => x.Amount == dto.Amount);
		if (dto.ShebaNumber is not null) q = q.Where(x => x.ShebaNumber == dto.ShebaNumber);
		if (dto.TransactionType is not null) q = q.Where(x => x.TransactionType == dto.TransactionType);
		if (dto.OrderId is not null) q = q.Where(x => x.OrderId == dto.OrderId);
		if (dto.GatewayName is not null) q = q.Where(x => x.GatewayName == dto.GatewayName);
		if (dto.PaymentId is not null) q = q.Where(x => x.PaymentId == dto.PaymentId);

		return new GenericResponse<IQueryable<TransactionEntity>>(q.AsNoTracking());
	}

	public GenericResponse<IQueryable<TransactionEntity>> ReadMine() => new(_dbContext.Set<TransactionEntity>().Where(i => i.UserId == _userId).AsNoTracking());
}