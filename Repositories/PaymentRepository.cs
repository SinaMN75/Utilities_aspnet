namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string>> IncreaseWalletBalance(long amount);
	Task<GenericResponse<string?>> PayOrder(Guid orderId);
	Task<GenericResponse<NgHostedResponse>> PayNg(NgPayDto dto);
	Task<GenericResponse<ZibalRequestReadDto>> PayZibal(NgPayDto dto);
	Task<GenericResponse<NgVerifyResponse>> VerifyNg(string outlet, string id);
	Task<GenericResponse> CallBack(int tagPayment, string id, long trackId);
	Task<GenericResponse> CallBackPremiumFake(int tagPayment, string id, long trackId);
}

public class PaymentRepository : IPaymentRepository {
	private readonly AppSettings _appSettings = new();
	private readonly DbContext _dbContext;
	private readonly string? _userId;
	private readonly ITransactionRepository _transactionRepository;
	private readonly IUserRepository _userRepository;
	private readonly IOrderRepository _orderRepository;

	public PaymentRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor,
		IConfiguration config,
		ITransactionRepository transactionRepository,
		IUserRepository userRepository,
		IOrderRepository orderRepository) {
		_dbContext = dbContext;
		config.GetSection("AppSettings").Bind(_appSettings);
		_userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
		_transactionRepository = transactionRepository;
		_userRepository = userRepository;
		_orderRepository = orderRepository;
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
				ZibalRequestReadDto? zibalRequestReadDto = await PaymentDataSource.PayZibal(new ZibalRequestCreateDto {
					Merchant = _appSettings.PaymentSettings.Id!,
					Amount = long.Parse(order.TotalPrice!.ToString()!),
					CallbackUrl = callbackUrl
				});
				return new GenericResponse<string?>(zibalRequestReadDto?.Result == 100
					? $"https://gateway.zibal.ir/start/{zibalRequestReadDto.TrackId}"
					: zibalRequestReadDto?.Message);
			}
		}

		return new GenericResponse<string?>("NULL");
	}

	public async Task<GenericResponse<NgHostedResponse>> PayNg(NgPayDto dto) =>
		new((await PaymentDataSource.PayNGenius(dto))!);

	public async Task<GenericResponse<ZibalRequestReadDto>> PayZibal(NgPayDto dto) {
		ZibalRequestReadDto response = (await PaymentDataSource.PayZibal(new ZibalRequestCreateDto {
			Merchant = dto.Outlet,
			Amount = dto.Amount ?? 0,
			CallbackUrl = dto.RedirectUrl
		}))!;

		return new GenericResponse<ZibalRequestReadDto>(response);
	}

	public async Task<GenericResponse<NgVerifyResponse>> VerifyNg(string outlet, string id) =>
		new((await PaymentDataSource.GetOrderStatus(outlet, id))!);

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
				ZibalVerifyReadDto? i = await PaymentDataSource.VerifyZibal(new ZibalVerifyCreateDto { Merchant = _appSettings.PaymentSettings.Id!, TrackId = trackId });
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

				await _transactionRepository.Create(new TransactionCreateDto {
					Amount = o.TotalPrice ?? 0,
					Descriptions = "خرید",
					RefId = refId,
					CardNumber = cardNumber,
					Tags = [TagTransaction.Buy],
					BuyerId = _userId,
					SellerId = productOwner.Id,
					OrderId = o.Id
				}, CancellationToken.None);

				await _dbContext.SaveChangesAsync();
				return new GenericResponse();
			}
			case 102: {
				break;
			}
			case 103: {
				OrderEntity o = (await _dbContext.Set<OrderEntity>()
					.Include(i => i.OrderDetails)!.ThenInclude(x => x.Product)
					.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id)))!;
				bool p1 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium1Month));
				bool p3 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium3Month));
				bool p6 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium6Month));
				bool p12 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium12Month));

				await _userRepository.Update(new UserCreateUpdateDto {
					Id = _userId,
					PremiumExpireDate =
						p1 ? DateTime.Now.AddMonths(1) :
						p3 ? DateTime.Now.AddMonths(3) :
						p6 ? DateTime.Now.AddMonths(6) :
						p12 ? DateTime.Now.AddMonths(12) : DateTime.Now
				});

				await _transactionRepository.Create(new TransactionCreateDto {
					Amount = o.TotalPrice ?? 0,
					Descriptions = "خرید اشتراک",
					RefId = refId,
					CardNumber = cardNumber,
					Tags = [TagTransaction.Premium],
					BuyerId = _userId,
					OrderId = o.Id
				}, CancellationToken.None);
				break;
			}
		}

		return new GenericResponse();
	}

	public async Task<GenericResponse> CallBackPremiumFake(int tagPayment, string id, long trackId) {
		OrderEntity o = (await _dbContext.Set<OrderEntity>()
			.Include(i => i.OrderDetails)!.ThenInclude(x => x.Product)
			.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id)))!;
		bool p1 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium1Month));
		bool p3 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium3Month));
		bool p6 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium6Month));
		bool p12 = o.OrderDetails!.Any(x => x.Product!.Tags.Contains(TagProduct.Premium12Month));

		await _orderRepository.Update(new OrderCreateUpdateDto {
			Id = o.Id,
			Tags = [TagOrder.Complete, TagOrder.Premium]
		});

		await _userRepository.Update(new UserCreateUpdateDto {
			Id = _userId,
			PremiumExpireDate =
				p1 ? DateTime.Now.AddMonths(1) :
				p3 ? DateTime.Now.AddMonths(3) :
				p6 ? DateTime.Now.AddMonths(6) :
				p12 ? DateTime.Now.AddMonths(12) : DateTime.Now
		});

		await _transactionRepository.Create(new TransactionCreateDto {
			Amount = o.TotalPrice ?? 0,
			Descriptions = "خرید اشتراک",
			RefId = "refId",
			CardNumber = "cardNumber",
			Tags = [TagTransaction.Premium],
			BuyerId = _userId,
			OrderId = o.Id
		}, CancellationToken.None);
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
				ZibalRequestReadDto? zibalRequestReadDto = await PaymentDataSource.PayZibal(new ZibalRequestCreateDto {
					Merchant = _appSettings.PaymentSettings.Id!,
					Amount = amount,
					CallbackUrl = callbackUrl
				});
				return new GenericResponse<string>(zibalRequestReadDto?.Result == 100
					? $"https://gateway.zibal.ir/start/{zibalRequestReadDto.TrackId}"
					: zibalRequestReadDto?.Message ?? "");
			}
		}

		return new GenericResponse<string>("");
	}
}