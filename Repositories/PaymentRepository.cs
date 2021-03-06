namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string?>> IncreaseWalletBalance(double amount, string zarinPalMerchantId);
	Task<GenericResponse<string?>> BuyProduct(Guid productId, string zarinPalMerchantId);

	Task<GenericResponse> WalletCallBack(
		int amount,
		string authority,
		string status,
		string userId,
		string zarinPalMerchantId);

	Task<GenericResponse> CallBack(
		Guid productId,
		string authority,
		string status,
		string zarinPalMerchantId);
}

public class PaymentRepository : IPaymentRepository {
	private readonly DbContext _context;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public PaymentRepository(DbContext context, IHttpContextAccessor httpContextAccessor) {
		_context = context;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<GenericResponse<string?>> IncreaseWalletBalance(double amount, string zarinPalMerchantId) {
		string? userId = _httpContextAccessor.HttpContext?.User.Identity?.Name;

		try {
			UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
			Payment payment = new(zarinPalMerchantId, amount.ToInt());
			string callbackUrl = $"{Server.ServerAddress}/Payment/WalletCallBack/{user?.Id}/{amount}";
			string desc = $"شارژ کیف پول به مبلغ {amount}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;

			await _context.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = amount,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				UserId = userId,
				StatusId = TransactionStatus.Pending
			});
			await _context.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string? url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string?>(url, UtilitiesStatusCodes.Success);
			}
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
		}
		catch (Exception ex) {
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
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

		UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
		Payment? payment = new(zarinPalMerchantId, amount);
		if (!status.Equals("OK")) {
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		}
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = _context.Set<TransactionEntity>().FirstOrDefault(x => x.Authority == authority);
		if (pay != null) {
			pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
			pay.RefId = verify.RefId;
			pay.UpdatedAt = DateTime.Now;
			_context.Set<TransactionEntity>().Update(pay);
		}

		user.Wallet += amount;
		_context.Set<UserEntity>().Update(user);

		await _context.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<string?>> BuyProduct(Guid productId, string zarinPalMerchantId) {
		string? userId = _httpContextAccessor.HttpContext?.User.Identity?.Name;

		try {
			ProductEntity? product = await _context.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == productId);
			UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
			Payment? payment = new(zarinPalMerchantId, product.Price.ToInt());
			string? callbackUrl = $"{Server.ServerAddress}/Payment/CallBack/{productId}";
			string? desc = $"خرید محصول {product.Title}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;
			await _context.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = product.Price.ToInt(),
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				UserId = userId,
				ProductId = productId,
				StatusId = TransactionStatus.Pending
			});
			await _context.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string? url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string?>(url, UtilitiesStatusCodes.Success);
			}
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
		}
		catch (Exception ex) {
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
		}
	}

	public async Task<GenericResponse> CallBack(
		Guid productId,
		string authority,
		string status,
		string zarinPalMerchantId) {
		ProductEntity? product = await _context.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == productId);
		Payment? payment = new(zarinPalMerchantId, product.Price.ToInt());
		if (!status.Equals("OK")) {
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		}
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = _context.Set<TransactionEntity>().FirstOrDefault(x => x.Authority == authority);
		if (pay != null) {
			pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
			pay.RefId = verify.RefId;
			pay.UpdatedAt = DateTime.Now;
			_context.Set<TransactionEntity>().Update(pay);
		}

		await _context.SaveChangesAsync();
		return new GenericResponse();
	}
}