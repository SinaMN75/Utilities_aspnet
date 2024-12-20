namespace Utilities_aspnet.Repositories;

public interface IPaymentRepository {
	Task<GenericResponse<NgHostedResponse>> PayNg(NgPayDto dto);
	Task<GenericResponse<NgVerifyResponse>> VerifyNg(string outlet, string id);
	Task<GenericResponse<ZibalRequestResponse>> PayZibal(NgPayDto dto);
	Task<GenericResponse<ZibalVerifyResponse>> VerifyZibal(string outlet, string id);
}

public class PaymentRepository : IPaymentRepository {
	public async Task<GenericResponse<NgHostedResponse>> PayNg(NgPayDto dto) {
		NgAccessTokenResponse requestAccessToken = await GetNGeniusAccessToken();

		RestRequest requestPay = new(Method.POST);
		requestPay.AddHeader("Content-Type", "application/vnd.ni-payment.v2+json");
		requestPay.AddHeader("Authorization", $"Bearer {requestAccessToken.access_token}");
		requestPay.AddHeader("Accept", "application/vnd.ni-payment.v2+json");
		requestPay.AddJsonBody(new {
				dto.action,
				amount = new { currencyCode = dto.currency, value = dto.amount },
				dto.emailAddress,
				merchantAttributes = new { dto.redirectUrl, skipConfirmationPage = true }
			}
		);
		IRestResponse responsePay = await new RestClient($"https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/{dto.outlet}/orders").ExecuteAsync(requestPay);

		return new GenericResponse<NgHostedResponse>(responsePay.Content.DecodeJson<NgHostedResponse>());
	}

	public async Task<GenericResponse<ZibalRequestResponse>> PayZibal(NgPayDto dto) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/request", Method.POST);

		requestRequest.AddJsonBody(new {
			merchant = dto.outlet,
			dto.amount,
			callbackUrl = dto.redirectUrl
		});

		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);
		return new GenericResponse<ZibalRequestResponse>(responseRequest.Content.DecodeJson<ZibalRequestResponse>());
	}

	public async Task<GenericResponse<ZibalVerifyResponse>> VerifyZibal(string outlet, string id) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/verify", Method.POST);
		requestRequest.AddJsonBody(new { merchant = outlet, trackId = id });
		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);

		Console.WriteLine(responseRequest.Request.Body!.Value);
		return new GenericResponse<ZibalVerifyResponse>(responseRequest.Content.DecodeJson<ZibalVerifyResponse>());
	}

	public async Task<GenericResponse<NgVerifyResponse>> VerifyNg(string outlet, string id) {
		NgAccessTokenResponse requestAccessToken = await GetNGeniusAccessToken();

		RestRequest requestOrderStatus = new(Method.GET);
		requestOrderStatus.AddHeader("Authorization", $"Bearer {requestAccessToken.access_token}");
		IRestResponse responseRequest =
			await new RestClient($"https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/{outlet}/orders/{id}").ExecuteAsync(requestOrderStatus);
		return new GenericResponse<NgVerifyResponse>(responseRequest.Content.DecodeJson<NgVerifyResponse>());
	}

	private static async Task<NgAccessTokenResponse> GetNGeniusAccessToken() {
		RestRequest requestAccessToken = new(Method.POST);
		requestAccessToken.AddHeader("Content-Type", "application/vnd.ni-identity.v1+json");
		requestAccessToken.AddHeader("Authorization", "Basic OTUyOTFhMzgtZDhjNy00Y2I1LWE3NTQtMmRhYmU1ZWZlY2E1OmMxYTA0OWJlLThiMGItNDg2My05NzQwLTgwNTY1Mjc4MTNiYQ==");
		requestAccessToken.AddHeader("Accept", "application/vnd.ni-identity.v1+json");
		IRestResponse responseRequest = await new RestClient("https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token").ExecuteAsync(requestAccessToken);
		return responseRequest.Content.DecodeJson<NgAccessTokenResponse>();
	}
}