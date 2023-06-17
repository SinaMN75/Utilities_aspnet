namespace Utilities_aspnet.Entities;

[Table("Order")]
public class OrderEntity : BaseEntity {
	public string? Description { get; set; }
	public string? DiscountCode { get; set; }
	public string? ProductUseCase { get; set; }
	public string? PayNumber { get; set; }
	public OrderStatuses? Status { get; set; }
	public double? TotalPrice { get; set; }
	public double? DiscountPrice { get; set; }
	public int? DiscountPercent { get; set; }
	public double? SendPrice { get; set; }
	public SendType? SendType { get; set; }
	public PayType? PayType { get; set; }
	public DateTime? PayDateTime { get; set; }
	public DateTime? ReceivedDate { get; set; }

	public AddressEntity? Address { get; set; }
	public Guid? AddressId { get; set; }

	public UserEntity? User { get; set; }

	[ForeignKey(nameof(User))]
	public string? UserId { get; set; }

	public UserEntity? ProductOwner { get; set; }

	[ForeignKey(nameof(ProductOwner))]
	public string? ProductOwnerId { get; set; }

	public IEnumerable<OrderDetailEntity>? OrderDetails { get; set; }
}

[Table("OrderDetail")]
public class OrderDetailEntity : BaseEntity {
	public double? Price { get; set; }
	public double? UnitPrice { get; set; }
	public int? Count { get; set; }

	public OrderEntity? Order { get; set; }
	public Guid? OrderId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public CategoryEntity? Category { get; set; }
	public Guid? CategoryId { get; set; }
}

public class OrderCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Description { get; set; }
	public string? DiscountCode { get; set; }
	public DateTime? ReceivedDate { get; set; }
	public Guid? AddressId { get; set; }
	public OrderStatuses? Status { get; set; } = OrderStatuses.Pending;
	public PayType? PayType { get; set; } = Utilities.PayType.Online;
	public SendType? SendType { get; set; } = Utilities.SendType.Custom;
	public IEnumerable<OrderDetailCreateUpdateDto>? OrderDetails { get; set; }
}

public class OrderDetailCreateUpdateDto {
	public Guid? OrderId { get; set; }
	public Guid? OrderDetailId { get; set; }
	public Guid? ProductId { get; set; }
	public int? Count { get; set; }
	public Guid? Category { get; set; }
}

public class OrderFilterDto {
	public Guid? Id { get; set; }
	public bool? ShowProducts { get; set; } = false;
	public OrderStatuses? Status { get; set; }
	public SendType? SendType { get; set; }
	public PayType? PayType { get; set; }
	public string? PayNumber { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string? UserId { get; set; }
	public string? ProductOwnerId { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
}