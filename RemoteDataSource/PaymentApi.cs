namespace Utilities_aspnet.RemoteDataSource;

public static class PaymentApi {
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
}