namespace Utilities_aspnet.Entities;

internal static class Converter {
	public static readonly JsonSerializerSettings? Settings = new() {
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
	};
}

public class NgVerifyResponse {
	[JsonProperty("_id")]
	public string? Id { get; set; }

	[JsonProperty("_links")]
	public NgVerifyResponseLinks? Links { get; set; }

	[JsonProperty("type")]
	public string? Type { get; set; }

	[JsonProperty("action")]
	public string? Action { get; set; }

	[JsonProperty("amount")]
	public Amount? Amount { get; set; }

	[JsonProperty("language")]
	public string? Language { get; set; }

	[JsonProperty("merchantAttributes")]
	public MerchantAttributes? MerchantAttributes { get; set; }

	[JsonProperty("emailAddress")]
	public string? EmailAddress { get; set; }

	[JsonProperty("reference")]
	public Guid? Reference { get; set; }

	[JsonProperty("outletId")]
	public Guid? OutletId { get; set; }

	[JsonProperty("createDateTime")]
	public DateTimeOffset? CreateDateTime { get; set; }

	[JsonProperty("paymentMethods")]
	public PaymentMethods? PaymentMethods { get; set; }

	[JsonProperty("referrer")]
	public string? Referrer { get; set; }

	[JsonProperty("formattedAmount")]
	public string? FormattedAmount { get; set; }

	[JsonProperty("_embedded")]
	public Embedded? Embedded { get; set; }

	public static string ToJson(NgVerifyResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);

	public static NgVerifyResponse FromJson(string json) => JsonConvert.DeserializeObject<NgVerifyResponse>(json, Converter.Settings)!;
}

public class NgVerifyResponseLinks {
	[JsonProperty("cancel")]
	public Cury? Cancel { get; set; }

	[JsonProperty("cnp:payment-link")]
	public Cury? CnpPaymentLink { get; set; }

	[JsonProperty("payment-authorization")]
	public Cury? PaymentAuthorization { get; set; }

	[JsonProperty("self")]
	public Cury? Self { get; set; }

	[JsonProperty("tenant-brand")]
	public Cury? TenantBrand { get; set; }

	[JsonProperty("payment")]
	public Cury? Payment { get; set; }

	[JsonProperty("merchant-brand")]
	public Cury? MerchantBrand { get; set; }
}

public class MerchantAttributes {
	[JsonProperty("redirectUrl")]
	public Uri? RedirectUrl { get; set; }
}

public class NgPayDto {
	public string? Action { get; set; }
	public string? EmailAddress { get; set; }
	public required string Outlet { get; set; }
	public string? Currency { get; set; }
	public required string RedirectUrl { get; set; }
	public required long Amount { get; set; }
	public bool? SkipConfirmationPage { get; set; }
}

public class NgAccessTokenResponse {
	[JsonProperty("access_token")]
	public string? AccessToken { get; set; }

	[JsonProperty("expires_in")]
	public long? ExpiresIn { get; set; }

	[JsonProperty("refresh_expires_in")]
	public long? RefreshExpiresIn { get; set; }

	[JsonProperty("token_type")]
	public string? TokenType { get; set; }

	public static string ToJson(NgAccessTokenResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
	public static NgAccessTokenResponse FromJson(string json) => JsonConvert.DeserializeObject<NgAccessTokenResponse>(json, Converter.Settings)!;
}

public class NgHostedResponse {
	[JsonProperty("_id")]
	public string? Id { get; set; }

	[JsonProperty("_links")]
	public NgHostedResponseLinks? Links { get; set; }

	[JsonProperty("type")]
	public string? Type { get; set; }

	[JsonProperty("action")]
	public string? Action { get; set; }

	[JsonProperty("amount")]
	public Amount? Amount { get; set; }

	[JsonProperty("language")]
	public string? Language { get; set; }

	[JsonProperty("emailAddress")]
	public string? EmailAddress { get; set; }

	[JsonProperty("reference")]
	public Guid? Reference { get; set; }

	[JsonProperty("outletId")]
	public Guid? OutletId { get; set; }

	[JsonProperty("createDateTime")]
	public DateTimeOffset? CreateDateTime { get; set; }

	[JsonProperty("paymentMethods")]
	public PaymentMethods? PaymentMethods { get; set; }

	[JsonProperty("referrer")]
	public string? Referrer { get; set; }

	[JsonProperty("formattedAmount")]
	public string? FormattedAmount { get; set; }

	[JsonProperty("_embedded")]
	public Embedded? Embedded { get; set; }

	public static NgHostedResponse FromJson(string json) => JsonConvert.DeserializeObject<NgHostedResponse>(json, Converter.Settings)!;
	public static string ToJson(NgHostedResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

public class Amount {
	[JsonProperty("currencyCode")]
	public string? CurrencyCode { get; set; }

	[JsonProperty("value")]
	public long? Value { get; set; }
}

public class Embedded {
	[JsonProperty("payment")]
	public Payment[]? Payment { get; set; }
}

public class Payment {
	[JsonProperty("_id")]
	public string? Id { get; set; }

	[JsonProperty("_links")]
	public PaymentLinks? Links { get; set; }

	[JsonProperty("reference")]
	public Guid? Reference { get; set; }

	[JsonProperty("state")]
	public string? State { get; set; }

	[JsonProperty("amount")]
	public Amount? Amount { get; set; }

	[JsonProperty("updateDateTime")]
	public DateTimeOffset? UpdateDateTime { get; set; }

	[JsonProperty("outletId")]
	public Guid? OutletId { get; set; }

	[JsonProperty("orderReference")]
	public Guid? OrderReference { get; set; }
}

public class PaymentLinks {
	[JsonProperty("self")]
	public Cury? Self { get; set; }

	[JsonProperty("payment:card")]
	public Cury? PaymentCard { get; set; }

	[JsonProperty("payment:saved-card")]
	public Cury? PaymentSavedCard { get; set; }

	[JsonProperty("curies")]
	public Cury[]? Curies { get; set; }
}

public class Cury {
	[JsonProperty("name")]
	public string? Name { get; set; }

	[JsonProperty("href")]
	public string? Href { get; set; }

	[JsonProperty("templated")]
	public bool? Templated { get; set; }
}

public class NgHostedResponseLinks {
	[JsonProperty("cancel")]
	public Cury? Cancel { get; set; }

	[JsonProperty("cnp:payment-link")]
	public Cury? CnpPaymentLink { get; set; }

	[JsonProperty("payment-authorization")]
	public Cury? PaymentAuthorization { get; set; }

	[JsonProperty("self")]
	public Cury? Self { get; set; }

	[JsonProperty("tenant-brand")]
	public Cury? TenantBrand { get; set; }

	[JsonProperty("payment")]
	public Cury? Payment { get; set; }

	[JsonProperty("merchant-brand")]
	public Cury? MerchantBrand { get; set; }
}

public class PaymentMethods {
	[JsonProperty("card")]
	public string[]? Card { get; set; }
}

public class ZibalRequestResponse {
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string? Message { get; set; }

	[JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
	public long? Result { get; set; }

	[JsonProperty("trackId", NullValueHandling = NullValueHandling.Ignore)]
	public long? TrackId { get; set; }

	public string? PaymentLink => $"https://gateway.zibal.ir/start/{TrackId}";

	public static ZibalRequestResponse? FromJson(string json) => JsonConvert.DeserializeObject<ZibalRequestResponse>(json, Converter.Settings);

	public static string ToJson(ZibalRequestResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

public class ZibalVerifyResponse {
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string? Message { get; set; }

	[JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
	public long? Result { get; set; }

	[JsonProperty("paidAt", NullValueHandling = NullValueHandling.Ignore)]
	public string? PaidAt { get; set; }

	[JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
	public long? Amount { get; set; }

	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public long? Status { get; set; }

	[JsonProperty("refNumber", NullValueHandling = NullValueHandling.Ignore)]
	public long? RefNumber { get; set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	[JsonProperty("cardNumber", NullValueHandling = NullValueHandling.Ignore)]
	public string? CardNumber { get; set; }

	[JsonProperty("orderId", NullValueHandling = NullValueHandling.Ignore)]
	public long? OrderId { get; set; }

	public static ZibalVerifyResponse? FromJson(string json) => JsonConvert.DeserializeObject<ZibalVerifyResponse>(json, Converter.Settings);

	public static string ToJson(ZibalVerifyResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
}