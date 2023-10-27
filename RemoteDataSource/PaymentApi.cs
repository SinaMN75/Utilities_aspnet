namespace Utilities_aspnet.RemoteDataSource;

public class PaymentApi {
	public static async Task<string?> PayZibal(ZibalRequestCreateDto dto) {
		RestRequest requestRequest = new("https://gateway.zibal.ir/v1/request", Method.POST);
		requestRequest.AddJsonBody(ZibalRequestCreateDto.ToJson(new ZibalRequestCreateDto {
			Merchant = dto.Merchant,
			Amount = dto.Amount,
			CallbackUrl = dto.CallbackUrl
		}));
		requestRequest.AddHeader("Content-Type", "application/json");

		IRestResponse responseRequest = await new RestClient().ExecuteAsync(requestRequest);
		ZibalRequestReadDto? zibalRequestReadDto = ZibalRequestReadDto.FromJson(responseRequest.Content);
		return zibalRequestReadDto?.Result == 100
			? $"https://gateway.zibal.ir/start/{zibalRequestReadDto.TrackId}"
			: zibalRequestReadDto?.Message;
	}
}