namespace Utilities_aspnet.Entities;

[Table("Order")]
public class OrderEntity : BaseEntity {
	[MaxLength(500)]
	public string? Description { get; set; }

	[MaxLength(20)]
	public string? DiscountCode { get; set; }

	[MaxLength(20)]
	public string? PayNumber { get; set; }

	public int? TotalPrice { get; set; }
	public int? DiscountPrice { get; set; }
	public int? DiscountPercent { get; set; }
	public int? SendPrice { get; set; }
	public DateTime? PayDateTime { get; set; }
	public DateTime? ReceivedDate { get; set; }
	public DateTime? DeliverDate { get; set; }
	public AddressEntity? Address { get; set; }
	public Guid? AddressId { get; set; }

	public UserEntity? User { get; set; }

	[ForeignKey(nameof(User))]
	public string? UserId { get; set; }

	public UserEntity? ProductOwner { get; set; }

	[ForeignKey(nameof(ProductOwner))]
	public string? ProductOwnerId { get; set; }

	public IEnumerable<TransactionEntity>? Transactions { get; set; }
	public List<TagOrder> Tags { get; set; } = new();

	public IEnumerable<OrderDetailEntity>? OrderDetails { get; set; }

	public List<ReservationDays> DaysReserved { get; set; } = new();
}

[Table("OrderDetail")]
public class OrderDetailEntity : BaseEntity {
	public int? UnitPrice { get; set; }
	public int? Count { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public OrderEntity? Order { get; set; }

	public Guid? OrderId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public int? Vote { get; set; }
	public int? FinalPrice { get; set; }
}

public class OrderCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Description { get; set; }
	public string? DiscountCode { get; set; }
	public DateTime? ReceivedDate { get; set; }
	public Guid? AddressId { get; set; }
	public List<TagOrder>? Tags { get; set; }
	public List<TagOrder>? RemoveTags { get; set; }
	public List<TagOrder>? AddTags { get; set; }
}

public class OrderDetailCreateUpdateDto {
	public Guid? ProductId { get; set; }
	public int? Count { get; set; }
	public List<ReservationDays>? Days { get; set; }
}

public class ReserveCreateUpdateDto {
	public Guid? ProductId { get; set; }
	public List<ReservationDays>? Days { get; set; }
}

public class ApplyDiscountCodeOnOrderDto {
	public Guid? OrderId { get; set; }
	public string? Code { get; set; }
}

public class OrderFilterDto {
	public Guid? Id { get; set; }
	public string? PayNumber { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string? UserId { get; set; }
	public List<TagOrder>? Tags { get; set; }
	public string? ProductOwnerId { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
}

public class OrderVoteDto {
	public Guid? Id { get; set; }
	public int Vote { get; set; }
}