namespace Utilities_aspnet.Entities;

[Table("Subscriptions")]
public class SubscriptionEntity : BaseEntity {
	public required string Title { get; set; }
	public string? PaymentRefId { get; set; }
	public required DateTime ExpiresIn { get; set; }
	public List<TagSubscription> Tags { get; set; } = [];

	public SubscriptionJsonDetail JsonDetail { get; set; } = new();
	
	public string? UserId { get; set; }
	public UserEntity? User { get; set; }
}

public class SubscriptionJsonDetail {
	public long? Price { get; set; }
	public List<KeyValue>? KeyValues { get; set; } = [];
	public List<string>? StringList { get; set; } = [];
}

public class SubscriptionCreateDto {
	public required string Title { get; set; }
	public required string PaymentRefId { get; set; }
	public required DateTime ExpiresIn { get; set; }
	public required string UserId { get; set; }
	public required List<TagSubscription> Tags { get; set; }
	public long? Price { get; set; }
	public List<KeyValue> KeyValues { get; set; } = [];
	public List<string> StringList { get; set; } = [];
}

public class SubscriptionUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? PaymentRefId { get; set; }
	public string? UserId { get; set; }
	public long? Price { get; set; }
	public DateTime? ExpiresIn { get; set; }
	public List<TagSubscription>? Tags { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
	public List<string>? StringList { get; set; }
}

public class SubscriptionFilterDto : BaseFilterDto {
	public List<TagSubscription> Tags { get; set; } = [];
	public string? UserId { get; set; }
}