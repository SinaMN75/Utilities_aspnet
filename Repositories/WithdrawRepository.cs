namespace Utilities_aspnet.Repositories;

public interface IWithdrawRepository {
	Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto);
	GenericResponse<IQueryable<WithdrawEntity>> Filter(WithdrawalFilterDto dto);
}

public class WithdrawRepository : IWithdrawRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public WithdrawRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto) {
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId && f.Suspend != true);
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);

		if (user.Wallet <= dto.Amount) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);

		string? sheba = dto.ShebaNumber.GetShebaNumber();

		if (dto.Amount < 100000) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);
		if (dto.Amount > 5000000) return new GenericResponse(UtilitiesStatusCodes.MoreThanAllowedMoney);
		if (sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		WithdrawEntity withdraw = new() {
			Amount = dto.Amount,
			UserId = user.Id,
			CreatedAt = DateTime.Now,
			ShebaNumber = dto.ShebaNumber,
			WithdrawState = WithdrawState.Requested
		};

		await _dbContext.Set<WithdrawEntity>().AddAsync(withdraw);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse();
	}

	public GenericResponse<IQueryable<WithdrawEntity>> Filter(WithdrawalFilterDto dto) {
		IQueryable<WithdrawEntity> q = _dbContext.Set<WithdrawEntity>().OrderByDescending(o => o.CreatedAt);

		if (dto.WithdrawState.HasValue) q = q.Where(w => w.WithdrawState == dto.WithdrawState);

		return new GenericResponse<IQueryable<WithdrawEntity>>(q);
	}
}