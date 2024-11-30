namespace Utilities_aspnet.Entities;

[Table("Transactions")]
public class TransactionEntity : BaseEntity {
	public long? Amount { get; set; }

	[MaxLength(10)]
	public string? Code { get; set; } = new Random().Next(10000, 99999).ToString();

	[MaxLength(500)]
	public string? Descriptions { get; set; }

	[MaxLength(100)]
	public string? RefId { get; set; }

	[MaxLength(30)]
	public string? CardNumber { get; set; }

	[MaxLength(100)]
	public List<TagTransaction> Tags { get; set; } = [];

	public UserEntity? Buyer { get; set; }

	[ForeignKey(nameof(Buyer))]
	public string? BuyerId { get; set; }

	public UserEntity? Seller { get; set; }

	[ForeignKey(nameof(Seller))]
	public string? SellerId { get; set; }

	public OrderEntity? Order { get; set; }
	public Guid? OrderId { get; set; }
}

public class TransactionCreateDto {
	public required long Amount { get; set; }
	public required string Descriptions { get; set; } = "";
	public string? RefId { get; set; }
	public string? CardNumber { get; set; }
	public required List<TagTransaction>? Tags { get; set; }
	public string? BuyerId { get; set; }
	public string? SellerId { get; set; }
	public Guid? OrderId { get; set; }
}

public class TransactionUpdateDto {
	public required Guid Id { get; set; }
	public long? Amount { get; set; }
	public string? Descriptions { get; set; }
	public string? RefId { get; set; }
	public string? CardNumber { get; set; }
	public List<TagTransaction>? Tags { get; set; }
}

public class TransactionFilterDto {
	public long? Amount { get; set; }
	public string? RefId { get; set; }
	public List<TagTransaction>? Tags { get; set; }
	public string? BuyerId { get; set; }
	public string? SellerId { get; set; }
	public Guid? OrderId { get; set; }
	public DateTime? DateTimeStart { get; set; }
	public DateTime? DateTimeEnd { get; set; }
}