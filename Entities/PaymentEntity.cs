namespace Utilities_aspnet.Entities;

public class NgVerifyResponse {
	public string? _id { get; set; }
	public NgVerifyResponseLinks? _links { get; set; }
	public string? type { get; set; }
	public string? action { get; set; }
	public Amount? amount { get; set; }
	public string? language { get; set; }
	public MerchantAttributes? merchantAttributes { get; set; }
	public string? emailAddress { get; set; }
	public Guid? reference { get; set; }
	public Guid? outletId { get; set; }
	public string? createDateTime { get; set; }
	public PaymentMethods? paymentMethods { get; set; }
	public string? referrer { get; set; }
	public string? formattedAmount { get; set; }
	public Embedded? _embedded { get; set; }
}

public class NgVerifyResponseLinks {
	public Cury? cancel { get; set; }
	public Cury? cnp_payment_link { get; set; }
	public Cury? payment_authorization { get; set; }
	public Cury? self { get; set; }
	public Cury? tenant_brand { get; set; }
	public Cury? payment { get; set; }
	public Cury? merchant_brand { get; set; }
}

public class MerchantAttributes {
	public Uri? redirectUrl { get; set; }
}

public class NgPayDto {
	public string? action { get; set; }
	public string? emailAddress { get; set; }
	public required string outlet { get; set; }
	public string? currency { get; set; }
	public required string redirectUrl { get; set; }
	public required long amount { get; set; }
	public bool? skipConfirmationPage { get; set; }
}

public class NgAccessTokenResponse {
	public string? access_token { get; set; }
	public long? expires_in { get; set; }
	public long? refresh_expires_in { get; set; }
	public string? token_type { get; set; }
}

public class NgHostedResponse {
	public string? _id { get; set; }
	public NgHostedResponseLinks? _links { get; set; }
	public string? type { get; set; }
	public string? action { get; set; }
	public Amount? amount { get; set; }
	public string? language { get; set; }
	public string? emailAddress { get; set; }
	public Guid? reference { get; set; }
	public Guid? outletId { get; set; }
	public string? createDateTime { get; set; }
	public PaymentMethods? paymentMethods { get; set; }
	public string? referrer { get; set; }
	public string? formattedAmount { get; set; }
	public Embedded? _embedded { get; set; }
}

public class Amount {
	public string? currencyCode { get; set; }
	public long? value { get; set; }
}

public class Embedded {
	public Payment[]? payment { get; set; }
}

public class Payment {
	public string? _id { get; set; }
	public PaymentLinks? _links { get; set; }
	public Guid? reference { get; set; }
	public string? state { get; set; }
	public Amount? amount { get; set; }
	public string? updateDateTime { get; set; }
	public Guid? outletId { get; set; }
	public Guid? orderReference { get; set; }
}

public class PaymentLinks {
	public Cury? self { get; set; }
	public Cury? payment_card { get; set; }
	public Cury? payment_saved_card { get; set; }
	public Cury[]? curies { get; set; }
}

public class Cury {
	public string? name { get; set; }
	public string? href { get; set; }
	public bool? templated { get; set; }
}

public class NgHostedResponseLinks {
	public Cury? cancel { get; set; }
	public Cury? cnp_payment_link { get; set; }
	public Cury? payment_authorization { get; set; }
	public Cury? self { get; set; }
	public Cury? tenant_brand { get; set; }
	public Cury? payment { get; set; }
	public Cury? merchant_brand { get; set; }
}

public class PaymentMethods {
	public string[]? card { get; set; }
}

public class ZibalRequestResponse {
	public string? message { get; set; }
	public long? result { get; set; }
	public long? trackId { get; set; }

	public string? PaymentLink => $"https://gateway.zibal.ir/start/{trackId}";
}

public class ZibalVerifyResponse {
	public string? message { get; set; }
	public long? result { get; set; }
	public string? paidAt { get; set; }
	public long? amount { get; set; }
	public long? status { get; set; }
	public long? refNumber { get; set; }
	public string? description { get; set; }
	public string? cardNumber { get; set; }
	public long? orderId { get; set; }
}