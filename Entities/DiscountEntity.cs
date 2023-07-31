namespace Utilities_aspnet.Entities;

[Table("Discount")]
public class DiscountEntity : BaseEntity {
	[MaxLength(100)]
	public string Title { get; set; } = null!;

	[MaxLength(20)]
	public string Code { get; set; } = null!;

	public int DiscountPrice { get; set; }
	public int NumberUses { get; set; } = 1000;
	public DateTime StartDate { get; set; } = new(2020);
	public DateTime EndDate { get; set; } = new(2040);

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
}

public class DiscountFilterDto {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public int? DiscountPercent { get; set; }
	public int? NumberUses { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}