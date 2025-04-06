namespace Utilities_aspnet.Repositories;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AvreenIpgService(IHttpClientFactory httpClientFactory) {
	public async Task<IpgGetTokenResponse> GetTokenAsync(IpgGetTokenParams body) {
		HttpClient client = httpClientFactory.CreateClient("BypassSslValidation");
		HttpResponseMessage response = await client.PostAsJsonAsync("https://oa.avreenco.com:8080/api/ipg/site/getToken", body);
		response.EnsureSuccessStatusCode();

		string content = await response.Content.ReadAsStringAsync();
		IpgGetTokenResponse? getTokenResponse = JsonSerializer.Deserialize<IpgGetTokenResponse>(content);

		return getTokenResponse;
	}


	public string GetPaymentUrl(string token) {
		return $"https://pay.avreenco.com:8181/api/ipg/site/goPayment?token={token}&lang=fa";
	}

	public async Task<IpgRedirectResultData> ConfirmPaymentAsync(IpgRedirectResult data) {
		IpgRedirectResultData? redirectResultData = JsonSerializer.Deserialize<IpgRedirectResultData>(
			Encoding.UTF8.GetString(Convert.FromBase64String(data.redirectData))
		);

		HttpClient client = httpClientFactory.CreateClient("BypassSslValidation");
		HttpResponseMessage response = await client.PostAsJsonAsync("https://oa.avreenco.com:8080/api/ipg/site/confirmPay", new {
			redirectResultData.mid,
			redirectResultData.token,
			password = "bXRoMTIzNDU2"
		});

		response.EnsureSuccessStatusCode();
		string content = await response.Content.ReadAsStringAsync();
		Console.WriteLine(content);

		return redirectResultData;
	}
}

public class IpgRedirectResult {
	public string redirectData { get; set; }
}

public class IpgRedirectResultData {
	public string status { get; set; }
	public string token { get; set; }
	public string clientTxnId { get; set; }
	public string maskPan { get; set; }
	public string txnDate { get; set; }
	public string rrn { get; set; }
	public long stan { get; set; }
	public string merchantPrivateData { get; set; }
	public string txnType { get; set; }
	public string mid { get; set; }
	public string terminalId { get; set; }
	public long amount { get; set; }
	public string signature { get; set; }
}

public class IpgGetTokenParams {
	public string mid { get; set; }
	public string password { get; set; }
	public string localDateTime { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmss");
	public long amount { get; set; }
	public SaleInfo saleInfo { get; set; }
	public string clientTxnId { get; set; }
	public string terminalId { get; set; }
	public string redirectUrl { get; set; }
	public string txnType { get; set; }
	public string cardHolderUserId { get; set; }
	public string mobileNumber { get; set; }
	public string emailAddress { get; set; }
	public string merchantPrivateData { get; set; }
	public string signature { get; set; }
}

public class SaleInfo {
	public string referenceId { get; set; }
	public string invoiceNo { get; set; }
}

public class IpgGetTokenResponse {
	public string signiture { get; set; }
	public string token { get; set; }
}