﻿using Zarinpal;
using Zarinpal.Models;

namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<string?>> IncreaseWalletBalance(decimal amount, string zarinPalMerchantId);
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
	private readonly IMapper _mapper;

	public PaymentRepository(DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) {
		_context = context;
		_mapper = mapper;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<GenericResponse<string?>> IncreaseWalletBalance(decimal amount, string zarinPalMerchantId) {
		string userId = _httpContextAccessor.HttpContext?.User.Identity?.Name;

		try {
			UserEntity user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
			int Amount = Decimal.ToInt32(amount);
			Payment? payment = new Payment(zarinPalMerchantId, Amount);
			string? callbackUrl = string.Format("{0}/Payment/WalletCallBack/{1}/{2}", Server.ServerAddress, user?.Id, Amount);
			string? Desc = $"شارژ کیف پول به مبلغ {Amount}";
			//var result = payment.PaymentRequest(Desc, callbackUrl, "", _user.PhoneNumber).Result;
			PaymentRequestResponse? result = payment.PaymentRequest(Desc, callbackUrl, "", user?.PhoneNumber).Result;
			///todo
			///save to db 
			await _context.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = Amount,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = Desc,
				GatewayName = "ZarinPal",
				UserId = userId,
				//PayDateTime = DateTime.Now,
				StatusId = TransactionStatus.Pending
			});
			await _context.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string? url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string?>(url, UtilitiesStatusCodes.BadRequest);
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

		UserEntity? _user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
		//int Amount = Decimal.ToInt32(model.Price);
		int Amount = amount;
		Payment? payment = new Payment(zarinPalMerchantId, Amount);
		if (!status.Equals("OK")) {
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		}
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		//verify.RefId
		TransactionEntity? _pay = _context.Set<TransactionEntity>().FirstOrDefault(x => x.Authority == authority);
		_pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
		_pay.RefId = verify.RefId;
		_pay.UpdatedAt = DateTime.Now;
		_context.Set<TransactionEntity>().Update(_pay);

		//_user.Credit = _user.Credit + amount;
		_user.Wallet = _user.Wallet + amount;
		_context.Set<UserEntity>().Update(_user);

		_context.SaveChanges();
		return new GenericResponse();
	}

	public async Task<GenericResponse<string?>> BuyProduct(Guid productId, string zarinPalMerchantId) {
		string userId = _httpContextAccessor.HttpContext?.User.Identity?.Name;

		try {
			ProductEntity product = await _context.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == productId);
			UserEntity user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
			int Amount = Decimal.ToInt32(product.Price ?? 0);
			Payment? payment = new Payment(zarinPalMerchantId, Amount);
			string? callbackUrl = string.Format("{0}/Payment/CallBack/{1}", Server.ServerAddress, productId);
			string? Desc = $"خرید محصول {product.Title}";
			//var result = payment.PaymentRequest(Desc, callbackUrl, "", _user.PhoneNumber).Result;
			PaymentRequestResponse? result = payment.PaymentRequest(Desc, callbackUrl, "", user?.PhoneNumber).Result;
			///todo
			///save to db 
			await _context.Set<TransactionEntity>().AddAsync(new TransactionEntity {
				Amount = Amount,
				Authority = result.Authority,
				CreatedAt = DateTime.Now,
				Descriptions = Desc,
				GatewayName = "ZarinPal",
				UserId = userId,
				ProductId = productId,
				//PayDateTime = DateTime.Now,
				StatusId = TransactionStatus.Pending
			});
			await _context.SaveChangesAsync();

			if (result.Status == 100 && result.Authority.Length == 36) {
				string? url = $"https://www.zarinpal.com/pg/StartPay/{result.Authority}";
				return new GenericResponse<string?>(url, UtilitiesStatusCodes.BadRequest);
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
		int Amount = Decimal.ToInt32(product.Price ?? 0);
		//int Amount = amount;
		Payment? payment = new Payment(zarinPalMerchantId, Amount);
		if (!status.Equals("OK")) {
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		}
		PaymentVerificationResponse? verify = payment.Verification(authority).Result;
		//verify.RefId
		TransactionEntity? _pay = _context.Set<TransactionEntity>().FirstOrDefault(x => x.Authority == authority);
		_pay.StatusId = (TransactionStatus?) Math.Abs(verify.Status);
		_pay.RefId = verify.RefId;
		_pay.UpdatedAt = DateTime.Now;
		_context.Set<TransactionEntity>().Update(_pay);

		_context.SaveChanges();
		return new GenericResponse();
	}
}