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
		string callbackUrl = $"{Server.ServerAddress}/CallBack/{TagPayment.PayOrder.GetHashCode()}/{orderId}";
		OrderEntity order = (await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == orderId))!;

		switch (_appSettings.PaymentSettings.Provider) {
			case "ZarinPal": {
				break;
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
				productOwner.Wallet += order.TotalPrice;
				_dbContext.Update(productOwner);

				if (order.OrderDetails != null)
					foreach (OrderDetailEntity? item in order.OrderDetails) {
						ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == item.ProductId))!;
						p.Stock -= item.Count;
						_dbContext.Update(p);
					}

				if (order.JsonDetail.DaysReserved.IsNotNullOrEmpty()) {
					ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == order.JsonDetail.DaysReserved.First().ProductId))!;
					p.JsonDetail.DaysAvailable.Where(x => x.ReserveId == order.JsonDetail.DaysReserved.First().ReserveId);
					p.JsonDetail.DaysReserved.AddRange(p.JsonDetail.DaysAvailable.Where(x => x.ReserveId == order.JsonDetail.DaysReserved.First().ReserveId));
					_dbContext.Update(p);
				}
				
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
		SubscriptionPaymentEntity spe = (await _dbContext.Set<SubscriptionPaymentEntity>().FirstOrDefaultAsync(x => x.Id == subscriptionId))!;
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId);
		string callbackUrl = $"{Server.ServerAddress}/CallBackSubscription/{spe.Id}";
		return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest);
	}

	public async Task<GenericResponse> CallBackSubscription(Guid subscriptionId, string authority, string status) {
		SubscriptionPaymentEntity spe = (await _dbContext.Set<SubscriptionPaymentEntity>().Include(i => i.Promotion)
			.FirstOrDefaultAsync(x => x.Id == subscriptionId))!;
		if (!status.Equals("OK")) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		spe.Tag = TagOrder.Complete;

		_dbContext.Update(spe);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}