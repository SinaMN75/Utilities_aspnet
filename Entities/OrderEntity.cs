namespace Utilities_aspnet.Entities;

[Table("Order")]
public class OrderEntity : BaseEntity {
	[MaxLength(500)]
	public string? Description { get; set; }

	[MaxLength(20)]
	public string? DiscountCode { get; set; }

	[MaxLength(20)]
	public string? PayNumber { get; set; }

	public required int OrderNumber { get; set; }

	public long? TotalPrice { get; set; }
	public long? DiscountPrice { get; set; }
	public int? DiscountPercent { get; set; }
	public long? SendPrice { get; set; }
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

	public OrderJsonDetail JsonDetail { get; set; } = new();
}

[Table("OrderDetail")]
public class OrderDetailEntity : BaseEntity {
	public long? UnitPrice { get; set; }
	public int? Count { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public OrderEntity? Order { get; set; }

	public Guid? OrderId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public int? Vote { get; set; }
	public long? FinalPrice { get; set; }
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
}

public class ReserveCreateUpdateDto {
	public required Guid ProductId { get; set; }
	public required List<ReserveDto> ReserveDto { get; set; }
}

public class ReserveChairCreateUpdateDto {
	public required Guid ProductId { get; set; }
	public required List<ReserveChairDto> ReserveChair { get; set; }
}

public class OrderJsonDetail {
	public List<OrderDetailHistory> OrderDetailHistories { get; set; } = new();
	public List<ReserveDto> ReservationTimes { get; set; } = new();
	public List<ReserveChairDto> ReserveChairs { get; set; } = new();
}

public class OrderDetailHistory {
	public string? ProductId { get; set; }
	public string? Title { get; set; }
	public int? Count { get; set; }
	public long? UnitPrice { get; set; }
	public long? FinalPrice { get; set; }
}

public class ReserveDto {
	public required string ReserveId { get; set; }
	public required Guid ProductId { get; set; }
	public required DateTime DateFrom { get; set; }
	public required DateTime DateTo { get; set; }
	public string? UserId { get; set; }
	public string? UserName { get; set; }
	public required int MemberCount { get; set; }
	public required int ExtraMemberCount { get; set; }
	public required long Price { get; set; }
	public required long PriceForAnyExtra { get; set; }
}

public class ReserveChairDto {
	public required string ChairId { get; set; }
	public required string SaloonId { get; set; }
	public required string SectionId { get; set; }
	public required DateTime DateFrom { get; set; }
	public required DateTime DateTo { get; set; }
	public string? UserId { get; set; }
	public string? UserName { get; set; }
	public required long Price { get; set; }
}

public class ApplyDiscountCodeOnOrderDto {
	public Guid? OrderId { get; set; }
	public string? Code { get; set; }
}

public class OrderFilterDto : BaseFilterDto {
	public Guid? Id { get; set; }
	public string? PayNumber { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string? UserId { get; set; }
	public List<TagOrder>? Tags { get; set; }
	public string? ProductOwnerId { get; set; }
}

public class OrderVoteDto {
	public Guid? Id { get; set; }
	public int Vote { get; set; }
}