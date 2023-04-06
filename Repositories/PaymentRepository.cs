namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string?>> IncreaseWalletBalance(double amount, string zarinPalMerchantId);
	Task<GenericResponse<string?>> PayOrderZarinPal(Guid orderId, string zarinPalMerchantId);
	Task<GenericResponse> WalletCallBack(int amount, string authority, string status, string userId, string zarinPalMerchantId);
	Task<GenericResponse> CallBack(Guid orderId, string authority, string status, string zarinPalMerchantId);
}

public class PaymentRepository : IPaymentRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public PaymentRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
	}

	public async Task<GenericResponse<string?>> IncreaseWalletBalance(double amount, string zarinPalMerchantId) {
		try {
			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
			Payment payment = new(zarinPalMerchantId, amount.ToInt());
			string callbackUrl = $"{Server.ServerAddress}/Payment/WalletCallBack/{user?.Id}/{amount}";
			string desc = $"شارژ کیف پول به مبلغ {amount}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;

			await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = amount,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				UserId = _userId,
				StatusId = TransactionStatus.Pending
			});
			await _dbContext.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string?>(url);
			}
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
		}
		catch (Exception ex) {
			return new GenericResponse<string?>(ex.Message, UtilitiesStatusCodes.BadRequest);
		}
	}

	public async Task<GenericResponse<string?>> PayOrderZarinPal(Guid orderId, string zarinPalMerchantId) {
		try {
			OrderEntity order = (await _dbContext.Set<OrderEntity>().FirstOrDefaultAsync(x => x.Id == orderId))!;
			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
			Payment payment = new(zarinPalMerchantId, order.TotalPrice.ToInt());
			string callbackUrl = $"{Server.ServerAddress}/Payment/CallBack/{orderId}";
			string desc = $"خرید محصول {order.Description}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;
			await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = order.TotalPrice.ToInt(),
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				UserId = _userId,
				OrderId = orderId,
				StatusId = TransactionStatus.Pending
			});
			await _dbContext.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string?>(url);
			}
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
		}
		catch (Exception ex) {
			return new GenericResponse<string?>(ex.Message, UtilitiesStatusCodes.BadRequest);
		}
	}

	public async Task<GenericResponse> WalletCallBack(
		int amount,
		string authority,
		string status,
		string userId,
		string zarinPalMerchantId) {
		if (userId.IsNullOrEmpty()) {
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		}

		UserEntity user = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId))!;
		Payment payment = new(zarinPalMerchantId, amount);
		if (!status.Equals("OK")) {
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		}
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = _dbContext.Set<TransactionEntity>().FirstOrDefault(x => x.Authority == authority);
		if (pay != null) {
			pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
			pay.RefId = verify.RefId;
			pay.UpdatedAt = DateTime.Now;
			_dbContext.Set<TransactionEntity>().Update(pay);
		}

		user.Wallet += amount;
		_dbContext.Set<UserEntity>().Update(user);

		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> CallBack(
		Guid orderId,
		string authority,
		string status,
		string zarinPalMerchantId) {
		OrderEntity order = (await _dbContext.Set<OrderEntity>().FirstOrDefaultAsync(x => x.Id == orderId))!;
		Payment payment = new(zarinPalMerchantId, order.TotalPrice.ToInt());
		if (!status.Equals("OK")) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = await _dbContext.Set<TransactionEntity>().FirstOrDefaultAsync(x => x.Authority == authority);
		if (pay != null) {
			pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
			pay.RefId = verify.RefId;
			pay.UpdatedAt = DateTime.Now;
			_dbContext.Set<TransactionEntity>().Update(pay);
		}
		order.Status = OrderStatuses.Paid;
		
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}