namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string>> PayProduct(Guid productId);
	Task<GenericResponse<string?>> IncreaseWalletBalance(int amount);
	Task<GenericResponse<string?>> PayOrder(Guid orderId);
	Task<GenericResponse<string?>> PaySubscription(Guid subscriptionId);
	Task<GenericResponse> WalletCallBack(int amount, string authority, string status, string userId);
	Task<GenericResponse> CallBack(Guid orderId, string authority, string status);
	Task<GenericResponse> CallBackSubscription(Guid subscriptionId, string authority, string status);
}

public class PaymentRepository : IPaymentRepository {
	private readonly AppSettings _appSettings = new();
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public PaymentRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration config) {
		_dbContext = dbContext;
		config.GetSection("AppSettings").Bind(_appSettings);
		_userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
	}

	public async Task<GenericResponse<string>> PayProduct(Guid productId) {
		try {
			ProductEntity p = (await _dbContext.Set<ProductEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId))!;
			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
			Payment payment = new(_appSettings.PaymentSettings.Id, p.Price!.Value);
			string callbackUrl = $"{Server.ServerAddress}/CallBack/{productId}";
			string desc = $"خرید محصول {p.Title}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;
			await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = p.Price,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				UserId = _userId,
			});
			await _dbContext.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string>(url);
			}

			return new GenericResponse<string>("", UtilitiesStatusCodes.BadRequest);
		}
		catch (Exception ex) {
			return new GenericResponse<string>(ex.Message, UtilitiesStatusCodes.BadRequest);
		}
	}

	public async Task<GenericResponse<string?>> IncreaseWalletBalance(int amount) {
		try {
			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
			Payment payment = new(_appSettings.PaymentSettings.Id, amount);
			string callbackUrl = $"{Server.ServerAddress}/WalletCallBack/{user?.Id}/{amount}";
			string desc = $"شارژ کیف پول به مبلغ {amount}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;

			await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = amount,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				// TransactionType = TransactionType.Recharge,
				UserId = _userId,
				// StatusId = TransactionStatus.Pending
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

	public async Task<GenericResponse<string?>> PayOrder(Guid orderId) {
		OrderEntity order = (await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == orderId))!;

		foreach (OrderDetailEntity orderDetail in order.OrderDetails!) {
			ProductEntity product = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == orderDetail.ProductId))!;
			if (product.Stock < orderDetail.Count) {
				await _dbContext.Set<OrderDetailEntity>().Where(i => i.Id == orderDetail.Id).ExecuteDeleteAsync();
			}
		}

		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
		Payment payment = new(_appSettings.PaymentSettings.Id, order.TotalPrice!.Value);
		string callbackUrl = $"{Server.ServerAddress}/CallBack/{orderId}";
		string desc = $"خرید محصول {order.Description}";
		PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;
		await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
			Amount = order.TotalPrice,
			Authority = result.Authority,
			CreatedAt = DateTime.Now,
			// TransactionType = TransactionType.Buy,
			Descriptions = desc,
			GatewayName = "ZarinPal",
			UserId = _userId,
			OrderId = orderId,
			// StatusId = TransactionStatus.Pending
		});
		await _dbContext.SaveChangesAsync();

		if (result.Status == 100 && result.Authority.Length == 36) {
			string url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
			return new GenericResponse<string?>(url);
		}

		return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
	}

	public async Task<GenericResponse> WalletCallBack(
		int amount,
		string authority,
		string status,
		string userId) {
		if (userId.IsNullOrEmpty()) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		UserEntity user = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId))!;
		Payment payment = new(_appSettings.PaymentSettings.Id, amount);
		if (!status.Equals("OK")) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = _dbContext.Set<TransactionEntity>().FirstOrDefault(x => x.Authority == authority);
		if (pay != null) {
			// pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
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
		string status) {
		OrderEntity order = (await _dbContext.Set<OrderEntity>().Include(i => i.OrderDetails)!.ThenInclude(x => x.Product)
			.FirstOrDefaultAsync(x => x.Id == orderId))!;
		UserEntity productOwner = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == order.ProductOwnerId))!;
		Payment payment = new(_appSettings.PaymentSettings.Id, order.TotalPrice!.Value);
		if (!status.Equals("OK")) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = await _dbContext.Set<TransactionEntity>().FirstOrDefaultAsync(x => x.Authority == authority);
		if (pay != null) {
			// pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
			pay.RefId = verify.RefId;
			pay.UpdatedAt = DateTime.Now;
			_dbContext.Set<TransactionEntity>().Update(pay);
		}

		if (order.OrderDetails != null)
			foreach (OrderDetailEntity? item in order.OrderDetails) {
				ProductEntity? prdct = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == item.ProductId);
				if (prdct is not null) {
					prdct.Stock = prdct.Stock >= 1 && prdct.Stock >= item.Count ? prdct.Stock -= item.Count : prdct.Stock;
					//Todo : inja ye exception hast ke nabayad rokh bede va pardakht ham anjam shode nemitonim throw konim 
					//       double check  ghabl az vorod be inn safhe okaye vali mitarsam ham zaman ye filed update beshe
					//       fek konamm bayad az ye transaction estefade konim
					_dbContext.Update(prdct);
				}

				item.FinalPrice = item.Product != null ? item.Product.Price : item.FinalPrice;
				_dbContext.Update(item);
			}

		if (order.AddressId is not null) {
			AddressEntity? address = await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == order.AddressId);
			if (address is not null) {
				address.IsDefault = true;
				_dbContext.Update(address);
				foreach (AddressEntity? item in _dbContext.Set<AddressEntity>().Where(w => w.UserId == address.UserId && w.Id != address.Id)) {
					item.IsDefault = false;
					_dbContext.Update(item);
				}
			}
		}

		productOwner.Wallet += order.TotalPrice;

		_dbContext.Update(productOwner);
		_dbContext.Update(order);
		TransactionEntity prOwnerTransaction = new() {
			Amount = order.TotalPrice,
			Authority = authority,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			Descriptions = $"پرداختی بابت سفارش محصول {order.OrderDetails!.Select(s => s.Product!.Title).ToList()}",
			GatewayName = pay?.GatewayName ?? "",
			OrderId = order.Id,
			// TransactionType = TransactionType.Sell,
			UserId = productOwner.Id
		};
		await _dbContext.AddAsync(prOwnerTransaction);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<string?>> PaySubscription(Guid subscriptionId) {
		try {
			SubscriptionPaymentEntity spe = (await _dbContext.Set<SubscriptionPaymentEntity>().FirstOrDefaultAsync(x => x.Id == subscriptionId))!;

			UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
			Payment payment = new(_appSettings.PaymentSettings.Id, (int)spe.Amount!.Value);
			string callbackUrl = $"{Server.ServerAddress}/CallBackSubscription/{spe.Id}";
			string desc = spe.PromotionId != null ? $"پروموشن {spe.Description}" : $"ارتقا اکانت {spe.Description}";
			PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;
			await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = (int)spe.Amount,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				// TransactionType = TransactionType.Recharge,
				Descriptions = desc,
				GatewayName = "ZarinPal",
				UserId = _userId,
				SubscriptionId = spe.Id,
				// StatusId = TransactionStatus.Pending
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

	public async Task<GenericResponse> CallBackSubscription(
		Guid subscriptionId,
		string authority,
		string status) {
		SubscriptionPaymentEntity spe =
			(await _dbContext.Set<SubscriptionPaymentEntity>().Include(i => i.Promotion).FirstOrDefaultAsync(x => x.Id == subscriptionId))!;
		Payment payment = new(_appSettings.PaymentSettings.Id, (int)spe.Amount!.Value);
		if (!status.Equals("OK")) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		TransactionEntity? pay = await _dbContext.Set<TransactionEntity>().FirstOrDefaultAsync(x => x.Authority == authority);
		if (pay != null) {
			// pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
			pay.RefId = verify.RefId;
			pay.UpdatedAt = DateTime.Now;
			_dbContext.Set<TransactionEntity>().Update(pay);
		}

		spe.Tag = TagOrder.Complete;

		_dbContext.Update(spe);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}