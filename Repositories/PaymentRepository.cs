using Utilities_aspnet.RemoteDataSource;

namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string>> IncreaseWalletBalance(long amount);
	Task<GenericResponse<string?>> PayOrder(Guid orderId);
	Task<GenericResponse<string?>> PaySubscription(Guid subscriptionId);
	Task<GenericResponse> CallBack(int tagPayment, string id, long trackId);
	Task<GenericResponse> CallBackFake(int tagPayment, string id, long trackId);
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
				await new RestClient("https://paymentpol.com/webservice/rest/PaymentRequest").ExecuteAsync(request);
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

	public async Task<GenericResponse> CallBack(int tagPayment, string id, long trackId) {
		long amount = 0;
		string refId = "";
		string cardNumber = "";
		switch (_appSettings.PaymentSettings.Provider) {
			case "ZarinPal": {
				break;
			}
			case "PaymentPol": {
				break;
			}
			case "Zibal": {
				ZibalVerifyReadDto? i = await PaymentApi.VerifyZibal(new ZibalVerifyCreateDto { Merchant = _appSettings.PaymentSettings.Id!, TrackId = trackId });
				amount = i?.Amount ?? 0;
				refId = i?.RefNumber.ToString() ?? "";
				cardNumber = i?.CardNumber ?? "";
				break;
			}
		}

		switch (tagPayment) {
			case 101: {
				OrderEntity o = (await _dbContext.Set<OrderEntity>()
					.Include(i => i.OrderDetails)!.ThenInclude(x => x.Product)
					.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id)))!;
				UserEntity productOwner = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == o.ProductOwnerId))!;
				productOwner.Wallet += o.TotalPrice;
				_dbContext.Update(productOwner);

				if (o.OrderDetails != null)
					foreach (OrderDetailEntity? item in o.OrderDetails) {
						ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == item.ProductId))!;
						p.Stock -= item.Count;
						_dbContext.Update(p);

						o.JsonDetail.OrderDetailHistories.Add(new OrderDetailHistory {
							ProductId = item.ProductId.ToString(),
							Count = item.Count,
							Title = (item.Product?.Parent?.Title ?? "") + " " + (item.Product?.Title ?? ""),
							UnitPrice = item.UnitPrice,
							FinalPrice = item.FinalPrice
						});
					}

				_dbContext.Update(o);

				await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
					Amount = o.TotalPrice,
					Descriptions = $"خرید",
					RefId = refId,
					CardNumber = cardNumber,
					Tags = new List<TagTransaction> { TagTransaction.Buy },
					UserId = _userId,
					OrderId = o.Id
				});

				await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
					Amount = o.TotalPrice,
					Descriptions = $"فروش",
					RefId = refId,
					CardNumber = cardNumber,
					Tags = new List<TagTransaction> { TagTransaction.Buy },
					UserId = productOwner.Id,
					OrderId = o.Id
				});

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

	public async Task<GenericResponse> CallBackFake(int tagPayment, string id, long trackId) {
		long amount = 0;
		string refId = "";
		string cardNumber = "";

		switch (tagPayment) {
			case 101: {
				OrderEntity o = (await _dbContext.Set<OrderEntity>()
					.Include(i => i.OrderDetails)!.ThenInclude(x => x.Product)
					.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id)))!;
				UserEntity productOwner = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == o.ProductOwnerId))!;
				productOwner.Wallet += o.TotalPrice;
				_dbContext.Update(productOwner);

				if (o.OrderDetails != null)
					foreach (OrderDetailEntity? item in o.OrderDetails) {
						ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == item.ProductId))!;
						p.Stock -= item.Count;
						_dbContext.Update(p);
					}

				if (o.JsonDetail.ReservationTimes.IsNotNullOrEmpty()) {
					ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == o.JsonDetail.ReservationTimes.First().ProductId))!;

					foreach (ReserveDto reserveDto in o.JsonDetail.ReservationTimes) {
						p.JsonDetail.ReservationTimes!.FirstOrDefault(x => x.ReserveId == reserveDto.ReserveId)!.ReservedByUserId = _userId;
						_dbContext.Set<ProductEntity>().Update(p);
					}
				}
				
				if (o.JsonDetail.ReservationTimes.IsNotNullOrEmpty()) {
					ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == Guid.Parse(o.JsonDetail.ProductId)))!;

					foreach (Seat reserveDto in o.JsonDetail.Seats) {
						p.JsonDetail.ReservationTimes!.FirstOrDefault(x => x.ReserveId == reserveDto.SeatId)!.ReservedByUserId = _userId;
						_dbContext.Set<ProductEntity>().Update(p);
					}
				}

				_dbContext.Update(o);

				await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
					Amount = o.TotalPrice,
					Descriptions = $"خرید",
					RefId = refId,
					CardNumber = cardNumber,
					Tags = new List<TagTransaction>() { TagTransaction.Buy },
					UserId = _userId,
					OrderId = o.Id
				});

				await _dbContext.Set<TransactionEntity>().AddAsync(new TransactionEntity {
					Amount = o.TotalPrice,
					Descriptions = $"فروش",
					RefId = refId,
					CardNumber = cardNumber,
					Tags = new List<TagTransaction>() { TagTransaction.Buy },
					UserId = productOwner.Id,
					OrderId = o.Id
				});

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