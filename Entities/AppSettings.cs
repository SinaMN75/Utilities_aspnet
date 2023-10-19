namespace Utilities_aspnet.Entities;

public class AppSettings {
	public SmsPanelSettings SmsPanelSettings { get; set; } = null!;
	public PaymentSettings PaymentSettings { get; set; } = null!;
	public PushNotificationSetting PushNotificationSetting { get; set; } = null!;
	public UsageRules UsageRules { get; set; } = null!;
	public int? WithdrawalLimit { get; set; }
	public int? WithdrawalTimeLimit { get; set; }
	public string? AndroidMinimumVersion { get; set; }
	public string? AndroidLatestVersion { get; set; }
	public string? IosMinimumVersion { get; set; }
	public string? IosLatestVersion { get; set; }
	public string? AndroidDownloadLink1 { get; set; }
	public string? AndroidDownloadLink2 { get; set; }
	public string? IosDownloadLink1 { get; set; }
	public string? IosDownloadLink2 { get; set; }
}

public class SmsPanelSettings {
	public string? Provider { get; set; }
	public string? UserName { get; set; }
	public string? SmsApiKey { get; set; }
	public string? SmsSecret { get; set; }
	public string? PatternCode { get; set; }
}

public class UsageRules {
	public int MaxProductPerDay { get; set; }
	public int MaxCommentPerDay { get; set; }
	public int MaxChatPerDay { get; set; }
}

public class PaymentSettings {
	public string? Id { get; set; }
	public string? Provider { get; set; }
}

public class PushNotificationSetting {
	public string? Provider { get; set; }
	public string? Token { get; set; }
	public string? AppId { get; set; }
}

public class IdTitleDto {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class EnumDto {
	public DateTime? DateTime { get; set; }
	public IEnumerable<IdTitleDto>? FormFieldType { get; set; }
	public IEnumerable<IdTitleDto>? TransactionStatuses { get; set; }
	public IEnumerable<IdTitleDto>? UtilitiesStatusCodes { get; set; }
	public IEnumerable<IdTitleDto>? Currency { get; set; }
	public IEnumerable<IdTitleDto>? SeenStatus { get; set; }
	public IEnumerable<IdTitleDto>? Priority { get; set; }
	public IEnumerable<IdTitleDto>? ChatStatus { get; set; }
	public IEnumerable<IdTitleDto>? Reaction { get; set; }
	public IEnumerable<IdTitleDto>? AgeCategory { get; set; }
	public IEnumerable<IdTitleDto>? ChatTypes { get; set; }
	public IEnumerable<IdTitleDto>? GenderType { get; set; }
	public IEnumerable<IdTitleDto>? PrivacyType { get; set; }
	public IEnumerable<IdTitleDto>? Nationality { get; set; }
	public IEnumerable<IdTitleDto>? LegalAuthenticationType { get; set; }
	public IEnumerable<IdTitleDto>? TagProduct { get; set; }
	public IEnumerable<IdTitleDto>? TagCategory { get; set; }
	public IEnumerable<IdTitleDto>? TagOrder { get; set; }
	public IEnumerable<IdTitleDto>? TagContent { get; set; }
	public IEnumerable<IdTitleDto>? TagNotification { get; set; }
	public IEnumerable<IdTitleDto>? TagMedia { get; set; }
	public IEnumerable<IdTitleDto>? TransactionType { get; set; }
	public IEnumerable<IdTitleDto>? TagComments { get; set; }
	public AppSettings? AppSettings { get; set; }
}

public class DashboardReadDto {
	public int Categories { get; set; }
	public int Products { get; set; }
	public int Users { get; set; }
	public int Orders { get; set; }
	public int Media { get; set; }
	public int Transactions { get; set; }
	public int Reports { get; set; }
	public int Address { get; set; }
	public int ReleasedProducts { get; set; }
	public int InQueueProducts { get; set; }
	public int NotAcceptedProducts { get; set; }
}