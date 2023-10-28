using Utilities_aspnet.RemoteDataSource;

namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string>> IncreaseWalletBalance(long amount);
	Task<GenericResponse<string?>> PayOrder(Guid orderId);
	Task<GenericResponse<string?>> PaySubscription(Guid subscriptionId);
	Task<GenericResponse> CallBack(int tagPayment, string id, int success, int status, long trackId);
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

	public async Task<GenericResponse<string?>> PayOrder(Guid orderId) {
		OrderEntity order = (await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == orderId))!;

		foreach (OrderDetailEntity orderDetail in order.OrderDetails!) {
			ProductEntity product = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == orderDetail.ProductId))!;
			if (product.Stock < orderDetail.Count)
				await _dbContext.Set<OrderDetailEntity>().Where(i => i.Id == orderDetail.Id).ExecuteDeleteAsync();
		}

		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
		string callbackUrl = $"{Server.ServerAddress}/CallBack/{TagPayment.PayOrder.GetHashCode()}/{orderId}";
		string desc = order.Description ?? "";

		switch (_appSettings.PaymentSettings.Provider) {
			case "ZarinPal": {
				Payment payment = new(_appSettings.PaymentSettings.Id, int.Parse(order.TotalPrice.ToString()));
				PaymentRequestResponse? result = payment.PaymentRequest(desc, callbackUrl, "", user?.PhoneNumber).Result;
				if (result.Status != 100 || result.Authority.Length != 36) return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
				return new GenericResponse<string?>($"https://www.zarinpal.com/pg/StartPay/{result.Authority}");
			}
			case "PaymentPol": {
				RestRequest request = new(Method.POST);
				request.AddParameter("MerchantID", _appSettings.PaymentSettings.Id!);
				request.AddParameter("Amount", order.TotalPrice!);
				request.AddParameter("CallbackURL", callbackUrl);
				IRestResponse response = await new RestClient("https://paymentpol.com/webservice/rest/PaymentRequest").ExecuteAsync(request);
				break;
			}
			case "Zibal": {
				ZibalRequestReadDto? zibalRequestReadDto = await PaymentApi.PayZibal(new ZibalRequestCreateDto {
					Merchant = _appSettings.PaymentSettings.Id!,
					Amount = long.Parse(order.TotalPrice!.ToString()!),
					CallbackUrl = callbackUrl,
				});
				return new GenericResponse<string?>(zibalRequestReadDto?.Result == 100
					? $"https://gateway.zibal.ir/start/{zibalRequestReadDto.TrackId}"
					: zibalRequestReadDto?.Message);
			}
		}

		return new GenericResponse<string?>("NULL");
	}

	public async Task<GenericResponse> CallBack(int tagPayment, string id, int success, int status, long trackId) {
		long amount = 0;
		switch (_appSettings.PaymentSettings.Provider) {
			case "ZarinPal": {
				break;
			}
			case "PaymentPol": {
				break;
			}
			case "Zibal": {
				ZibalVerifyReadDto? i = await PaymentApi.VerifyZibal(new ZibalVerifyCreateDto { Merchant = _appSettings.PaymentSettings.Id!, TrackId = trackId });
				amount = i.Amount ?? 0;
				break;
			}
		}

		switch (tagPayment) {
			case 101: {
				OrderEntity order = (await _dbContext.Set<OrderEntity>()
					.Include(i => i.OrderDetails)!.ThenInclude(x => x.Product)
					.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id)))!;
				UserEntity productOwner = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == order.ProductOwnerId))!;

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

				if (order.JsonDetail.DaysReserved.IsNotNullOrEmpty()) {
					ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == order.JsonDetail.DaysReserved.First().ProductId))!;
					p.JsonDetail.DaysAvailable.Where(x => x.ReserveId == order.JsonDetail.DaysReserved.First().ReserveId);
					p.JsonDetail.DaysReserved.AddRange(p.JsonDetail.DaysAvailable.Where(x => x.ReserveId == order.JsonDetail.DaysReserved.First().ReserveId));
					_dbContext.Update(p);
					await _dbContext.SaveChangesAsync();
				}

				productOwner.Wallet += order.TotalPrice;

				_dbContext.Update(productOwner);
				_dbContext.Update(order);
				await _dbContext.SaveChangesAsync();
				return new GenericResponse();
			}
			case 102: {
				UserEntity user = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!;
				user.Wallet += amount;
				_dbContext.Update(user);
				await _dbContext.SaveChangesAsync();
				break;
			}
			case 103: {
				await PaymentApi.VerifyZibal(new ZibalVerifyCreateDto {
					Merchant = _appSettings.PaymentSettings.Id!, TrackId = trackId,
				});
				break;
			}
		}

		return new GenericResponse();
	}

	public async Task<GenericResponse<string>> IncreaseWalletBalance(long amount) {
		string callbackUrl = $"{Server.ServerAddress}/CallBack/{TagPayment.PayWallet.GetHashCode()}/{_userId}";

		switch (_appSettings.PaymentSettings.Provider) {
			case "ZarinPal": {
				break;
			}
			case "PaymentPol": {
				break;
			}
			case "Zibal": {
				ZibalRequestReadDto? zibalRequestReadDto = await PaymentApi.PayZibal(new ZibalRequestCreateDto {
					Merchant = _appSettings.PaymentSettings.Id!,
					Amount = amount,
					CallbackUrl = callbackUrl,
				});
				return new GenericResponse<string>(zibalRequestReadDto?.Result == 100
					? $"https://gateway.zibal.ir/start/{zibalRequestReadDto.TrackId}"
					: zibalRequestReadDto?.Message ?? "");
			}
		}

		return new GenericResponse<string>("");
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

	public async Task<GenericResponse> CallBackSubscription(Guid subscriptionId, string authority, string status) {
		SubscriptionPaymentEntity spe = (await _dbContext.Set<SubscriptionPaymentEntity>().Include(i => i.Promotion)
			.FirstOrDefaultAsync(x => x.Id == subscriptionId))!;
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