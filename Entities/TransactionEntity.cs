namespace Utilities_aspnet.Entities;

[Table("Transactions")]
public class TransactionEntity : BaseEntity {
	public long? Amount { get; set; }

	[MaxLength(500)]
	public string? Descriptions { get; set; }

	[MaxLength(100)]
	public string? RefId { get; set; }

	[MaxLength(30)]
	public string? CardNumber { get; set; }

	[MaxLength(100)]
	public List<TagTransaction> Tags { get; set; } = new();

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	public OrderEntity? Order { get; set; }
	public Guid? OrderId { get; set; }

	public SubscriptionPaymentEntity? SubscriptionPayment { get; set; }
	public Guid? SubscriptionId { get; set; }
}

public class TransactionFilterDto {
	public long? Amount { get; set; }
	public string? RefId { get; set; }
	public List<TagTransaction>? Tags { get; set; }
	public string? UserId { get; set; }
	public Guid? OrderId { get; set; }
}