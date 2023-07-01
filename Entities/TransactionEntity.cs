namespace Utilities_aspnet.Entities;

[Table("Transactions")]
public class TransactionEntity : BaseEntity {
	public int? Amount { get; set; }

	[MaxLength(500)]
	public string? Descriptions { get; set; }

	[MaxLength(100)]
	public string? Authority { get; set; }

	[MaxLength(100)]
	public string? GatewayName { get; set; }

	[MaxLength(20)]
	public string? PaymentId { get; set; }

	[MaxLength(50)]
	public string? ShebaNumber { get; set; }

	public long? RefId { get; set; }
	public TransactionStatus? StatusId { get; set; } = TransactionStatus.Pending;
	public TransactionType? TransactionType { get; set; } = Utilities.TransactionType.None;

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	public OrderEntity? Order { get; set; }
	public Guid? OrderId { get; set; }
}