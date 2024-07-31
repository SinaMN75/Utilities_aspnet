namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<NgHostedResponse>> PayNg(NgPayDto dto);
	Task<GenericResponse<NgVerifyResponse>> VerifyNg(string outlet, string id);
	Task<GenericResponse<ZibalRequestResponse>> PayZibal(NgPayDto dto);
	Task<GenericResponse<ZibalVerifyResponse>> VerifyZibal(string outlet, string id);
	Task<GenericResponse> CallBackZibalJadooAuthorize(string outlet, string id, string userId);
}

public class PaymentRepository : IPaymentRepository {
	private readonly AppSettings _appSettings = new();
	private readonly DbContext _dbContext;
	private readonly IUserRepository _userRepository;

	public PaymentRepository(IConfiguration config, DbContext dbContext, IUserRepository userRepository) {
		_dbContext = dbContext;
		_userRepository = userRepository;
		config.GetSection("AppSettings").Bind(_appSettings);
	}

	public async Task<GenericResponse<NgHostedResponse>> PayNg(NgPayDto dto) {
		NgAccessTokenResponse requestAccessToken = await GetNGeniusAccessToken();

		RestRequest requestPay = new(Method.POST);
		requestPay.AddHeader("Content-Type", "application/vnd.ni-payment.v2+json");
		requestPay.AddHeader("Authorization", $"Bearer {requestAccessToken.AccessToken}");
		requestPay.AddHeader("Accept", "application/vnd.ni-payment.v2+json");
		requestPay.AddJsonBody(new {
				action = dto.Action,
				amount = new { currencyCode = dto.Currency, value = dto.Amount },
				emailAddress = dto.EmailAddress,
				merchantAttributes = new { redirectUrl = dto.RedirectUrl, skipConfirmationPage = true }
			}
		);
		IRestResponse responsePay = await new RestClient($"https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/{dto.Outlet}/orders").ExecuteAsync(requestPay);

		return new GenericResponse<NgHostedResponse>(NgHostedResponse.FromJson(responsePay.Content));
	}

	public async Task<GenericResponse<ZibalRequestResponse>> PayZibal(NgPayDto dto) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/request", Method.POST);

		requestRequest.AddJsonBody(new {
			merchant = dto.Outlet,
			amount = dto.Amount,
			callbackUrl = dto.RedirectUrl
		});

		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);
		return new GenericResponse<ZibalRequestResponse>(ZibalRequestResponse.FromJson(responseRequest.Content)!);
	}

	public async Task<GenericResponse<ZibalVerifyResponse>> VerifyZibal(string outlet, string id) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/verify", Method.POST);
		requestRequest.AddJsonBody(new { merchant = outlet, trackId = id });
		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);

		Console.WriteLine(responseRequest.Request.Body!.Value);
		return new GenericResponse<ZibalVerifyResponse>(ZibalVerifyResponse.FromJson(responseRequest.Content)!);
	}

	public async Task<GenericResponse<NgVerifyResponse>> VerifyNg(string outlet, string id) {
		NgAccessTokenResponse requestAccessToken = await GetNGeniusAccessToken();

		RestRequest requestOrderStatus = new(Method.GET);
		requestOrderStatus.AddHeader("Authorization", $"Bearer {requestAccessToken.AccessToken}");
		IRestResponse responseRequest =
			await new RestClient($"https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/{outlet}/orders/{id}").ExecuteAsync(requestOrderStatus);
		return new GenericResponse<NgVerifyResponse>(NgVerifyResponse.FromJson(responseRequest.Content));
	}

	private static async Task<NgAccessTokenResponse> GetNGeniusAccessToken() {
		RestRequest requestAccessToken = new(Method.POST);
		requestAccessToken.AddHeader("Content-Type", "application/vnd.ni-identity.v1+json");
		requestAccessToken.AddHeader("Authorization", "Basic OTUyOTFhMzgtZDhjNy00Y2I1LWE3NTQtMmRhYmU1ZWZlY2E1OmMxYTA0OWJlLThiMGItNDg2My05NzQwLTgwNTY1Mjc4MTNiYQ==");
		requestAccessToken.AddHeader("Accept", "application/vnd.ni-identity.v1+json");
		IRestResponse responseRequest = await new RestClient("https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token").ExecuteAsync(requestAccessToken);
		return NgAccessTokenResponse.FromJson(responseRequest.Content);
	}

	public async Task<GenericResponse> CallBackZibalJadooAuthorize(string outlet, string id, string userId) {
		GenericResponse<ZibalVerifyResponse> i = await VerifyZibal(outlet, id);

		UserEntity user = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId))!;
		List<TagUser> tags = user.Tags;
		tags.Add(TagUser.Authorized);
		await _userRepository.Update(new UserCreateUpdateDto { Tags = tags });

		return new GenericResponse();
	}
}