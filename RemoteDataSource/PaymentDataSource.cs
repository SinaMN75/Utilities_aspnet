namespace Utilities_aspnet.RemoteDataSource;

public static class PaymentDataSource {
	public static async Task<ZibalRequestReadDto?> PayZibal(ZibalRequestCreateDto dto) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/request", Method.POST);
		requestRequest.AddJsonBody(ZibalRequestCreateDto.ToJson(new ZibalRequestCreateDto {
			Merchant = dto.Merchant,
			Amount = dto.Amount,
			CallbackUrl = dto.CallbackUrl
		}));
		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);
		ZibalRequestReadDto? zibalRequestReadDto = ZibalRequestReadDto.FromJson(responseRequest.Content);
		return zibalRequestReadDto;
	}

	public static async Task<ZibalVerifyReadDto?> VerifyZibal(ZibalVerifyCreateDto dto) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/verify", Method.POST);
		requestRequest.AddJsonBody(ZibalVerifyCreateDto.ToJson(new ZibalVerifyCreateDto { Merchant = dto.Merchant, TrackId = dto.TrackId }));
		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);
		return ZibalVerifyReadDto.FromJson(responseRequest.Content);
	}

	public static async Task<NgeniusHostedResponse?> PayNGenius(NgeniusPaymentDto dto) {
		RestRequest requestAccessToken = new(Method.POST);
		requestAccessToken.AddHeader("Content-Type", "application/vnd.ni-identity.v1+json");
		requestAccessToken.AddHeader("Authorization", "Basic OTUyOTFhMzgtZDhjNy00Y2I1LWE3NTQtMmRhYmU1ZWZlY2E1OmMxYTA0OWJlLThiMGItNDg2My05NzQwLTgwNTY1Mjc4MTNiYQ==");
		requestAccessToken.AddHeader("Accept", "application/vnd.ni-identity.v1+json");
		IRestResponse responseRequest = await new RestClient("https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token").ExecuteAsync(requestAccessToken);
		NGeniusAccessTokenReadDto accessTokenReadDto = NGeniusAccessTokenReadDto.FromJson(responseRequest.Content);

		RestRequest requestPay = new(Method.POST);
		requestPay.AddHeader("Content-Type", "application/vnd.ni-payment.v2+json");
		requestPay.AddHeader("Authorization", $"Bearer {accessTokenReadDto.AccessToken}");
		requestPay.AddHeader("Accept", "application/vnd.ni-payment.v2+json");
		requestPay.AddJsonBody(new {
				action = dto.Action,
				amount = new { currencyCode = dto.Currency, value = dto.Amount },
				emailAddress = dto.EmailAddress,
				merchantAttributes = new {
					redirectUrl = dto.RedirectUrl
				}
			}
		);
		IRestResponse responsePay = await new RestClient($"https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/{dto.Outlet}/orders").ExecuteAsync(requestPay);

		return NgeniusHostedResponse.FromJson(responsePay.Content);
	}
}