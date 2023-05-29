namespace Utilities_aspnet.Repositories;

public interface ITransactionRepository {
	GenericResponse<IQueryable<TransactionEntity>> Read();
	GenericResponse<IQueryable<TransactionEntity>> ReadMine();
	Task<GenericResponse<TransactionEntity>> Create(TransactionEntity dto);
}

public class TransactionRepository : ITransactionRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public TransactionRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<TransactionEntity>> Create(TransactionEntity entity) {
		entity.UserId ??= _userId;
		await _dbContext.Set<TransactionEntity>().AddAsync(entity);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<TransactionEntity>(entity);
	}

	public GenericResponse<IQueryable<TransactionEntity>> Read() {
		return new GenericResponse<IQueryable<TransactionEntity>>(_dbContext.Set<TransactionEntity>());
	}

	public GenericResponse<IQueryable<TransactionEntity>> ReadMine() {
		IQueryable<TransactionEntity> i = _dbContext.Set<TransactionEntity>()
			.Where(i => i.UserId == _userId);
		return new GenericResponse<IQueryable<TransactionEntity>>(i);
	}
}