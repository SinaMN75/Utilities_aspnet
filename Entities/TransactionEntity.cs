namespace Utilities_aspnet.Entities;

[Table("Transactions")]
public class TransactionEntity : BaseEntity {
	public double? Amount { get; set; }
	public string? Descriptions { get; set; }
	public string? Authority { get; set; }
	public string? GatewayName { get; set; }
	public string? PaymentId { get; set; }
	public string? ShebaNumber { get; set; }
	public long? RefId { get; set; }
	public TransactionStatus? StatusId { get; set; } = TransactionStatus.Pending;

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	public OrderEntity? Order { get; set; }
	public Guid? OrderId { get; set; }
}