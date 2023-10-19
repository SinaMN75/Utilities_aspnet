namespace Utilities_aspnet.Repositories;

public interface IWithdrawRepository {
	Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto);
	GenericResponse<IQueryable<WithdrawEntity>> Filter(WithdrawalFilterDto dto);
	Task<GenericResponse<WithdrawEntity?>> Update(WithdrawCreateUpdateDto dto);
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

	[Time]
	public async Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto) {
		UserEntity user = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId && f.Suspend != true))!;

		if (user.Wallet <= dto.Amount || dto.Amount < 100000) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);

		string? sheba = dto.ShebaNumber.GetShebaNumber();

		IQueryable<WithdrawEntity> unDoneRequests = _dbContext.Set<WithdrawEntity>().Where(a => a.WithdrawState != WithdrawState.Requested).AsNoTracking();
		if (unDoneRequests.IsNotNullOrEmpty()) return new GenericResponse(UtilitiesStatusCodes.Overused);

		WithdrawEntity withdraw = new() {
			Amount = dto.Amount,
			UserId = user.Id,
			CreatedAt = DateTime.Now,
			ShebaNumber = sheba,
			WithdrawState = WithdrawState.Requested,
			UpdatedAt = DateTime.Now
		};
		await _dbContext.Set<WithdrawEntity>().AddAsync(withdraw);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	[Time]
	public GenericResponse<IQueryable<WithdrawEntity>> Filter(WithdrawalFilterDto dto) {
		IQueryable<WithdrawEntity> q = _dbContext.Set<WithdrawEntity>().AsNoTracking().OrderByDescending(o => o.CreatedAt);
		if (dto.State.HasValue) q = q.Where(w => w.WithdrawState == dto.State);
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == dto.UserId);
		return new GenericResponse<IQueryable<WithdrawEntity>>(q);
	}

	[Time]
	public async Task<GenericResponse<WithdrawEntity?>> Update(WithdrawCreateUpdateDto dto) {
		WithdrawEntity e = (await _dbContext.Set<WithdrawEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id))!;
		if (e.WithdrawState != WithdrawState.Requested)
			return new GenericResponse<WithdrawEntity?>(null, UtilitiesStatusCodes.OrderPayed, "امکان تغییر دادن دوباره وجود ندارد");
		e.WithdrawState = dto.WithdrawState;
		e.AdminMessage = dto.AdminMessage ?? "";
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<WithdrawEntity?>(e);
	}
}