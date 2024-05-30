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
		RestRequest request = new(Method.POST);
		request.AddHeader("content-type", "application/vnd.ni-identity.v1+json");
		request.AddHeader("authorization", "Basic OTUyOTFhMzgtZDhjNy00Y2I1LWE3NTQtMmRhYmU1ZWZlY2E1OmMxYTA0OWJlLThiMGItNDg2My05NzQwLTgwNTY1Mjc4MTNiYQ==");
		request.AddHeader("accept", "application/vnd.ni-identity.v1+json");
		IRestResponse responseRequest = await new RestClient("https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token").ExecuteAsync(request);
		NGeniusAccessTokenReadDto accessTokenReadDto = NGeniusAccessTokenReadDto.FromJson(responseRequest.Content);

		RestRequest requestPay = new(Method.POST);
		requestPay.AddHeader("content-type", "application/vnd.ni-payment.v2+json");
		requestPay.AddHeader("authorization", $"Bearer {accessTokenReadDto.AccessToken}");
		requestPay.AddHeader("accept", "application/vnd.ni-payment.v2+json");
		requestPay.AddBody(new {
				action = dto.Action,
				amount = new { currencyCode = dto.Currency, value = dto.Amount }
			}
		);
		IRestResponse responsePay = await new RestClient($"https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/{dto.Outlet}/orders").ExecuteAsync(request);

		return NgeniusHostedResponse.FromJson(responsePay.Content);
	}
}