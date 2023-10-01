namespace Utilities_aspnet.Repositories;

public interface IWithdrawRepository {
	Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto);
	GenericResponse<IQueryable<WithdrawEntity>> Filter(WithdrawalFilterDto dto);
}

public class WithdrawRepository : IWithdrawRepository {
	private readonly AppSettings _appSettings = new();
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public WithdrawRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration config) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
		config.GetSection("AppSettings").Bind(_appSettings);
	}

	public async Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto) {
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId && f.Suspend != true);
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);

		if (user.Wallet <= dto.Amount) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);

		string? sheba = dto.ShebaNumber.GetShebaNumber();

		if (dto.Amount < 100000) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);

		double limit = _appSettings.WithdrawalLimit;
		int dateLimit = _appSettings.WithdrawalTimeLimit;
		IQueryable<WithdrawEntity> listOfWithdrawalInIntervalDate = _dbContext.Set<WithdrawEntity>()
			.Where(a => a.CreatedAt > DateTime.Now.AddDays(-dateLimit) && a.CreatedAt < DateTime.Now && a.WithdrawState == WithdrawState.Accepted);
		int sumOfWithDrawal = listOfWithdrawalInIntervalDate.Sum(s => s.Amount) ?? 0;
		if (dto.Amount + sumOfWithDrawal > limit) return new GenericResponse(UtilitiesStatusCodes.MoreThanAllowedMoney);
		if (sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		WithdrawEntity withdraw = new() {
			Amount = dto.Amount,
			UserId = user.Id,
			CreatedAt = DateTime.Now,
			ShebaNumber = dto.ShebaNumber,
			WithdrawState = WithdrawState.Requested
		};
		await _dbContext.Set<WithdrawEntity>().AddAsync(withdraw);

		TransactionEntity transaction = new() {
			UserId = _userId,
			ShebaNumber = dto.ShebaNumber,
			Amount = dto.Amount,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};
		await _dbContext.Set<TransactionEntity>().AddAsync(transaction);

		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<WithdrawEntity>> Filter(WithdrawalFilterDto dto) {
		IQueryable<WithdrawEntity> q = _dbContext.Set<WithdrawEntity>().OrderByDescending(o => o.CreatedAt);

		if (dto.WithdrawState.HasValue) q = q.Where(w => w.WithdrawState == dto.WithdrawState);

		return new GenericResponse<IQueryable<WithdrawEntity>>(q);
	}
}