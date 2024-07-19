namespace Utilities_aspnet.Entities;

[Table("Discount")]
public class DiscountEntity : BaseEntity {
	[MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(20)]
	public required string Code { get; set; }

	public required long DiscountPrice { get; set; }
	public required int NumberUses { get; set; } = 1000;
	public required DateTime StartDate { get; set; } = new(2020);
	public required DateTime EndDate { get; set; } = new(2040);

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
}

public class DiscountCreateDto {
	public required string Title { get; set; } = null!;
	public required string Code { get; set; } = null!;

	public required long DiscountPrice { get; set; }
	public required int NumberUses { get; set; } = 1000;
	public required DateTime StartDate { get; set; } = new(2020);
	public required DateTime EndDate { get; set; } = new(2040);
	public string? UserId { get; set; }
}

public class DiscountUpdateDto {
	public required Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }

	public long? DiscountPrice { get; set; }
	public int? NumberUses { get; set; } = 1000;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}

public class DiscountFilterDto : BaseFilterDto {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public int? NumberUses { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}