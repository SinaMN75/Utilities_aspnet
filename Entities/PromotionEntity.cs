namespace Utilities_aspnet.Entities;

[Table("Promotion")]
public class PromotionEntity : BaseEntity {
	public DisplayType DisplayType { get; set; }

	[MaxLength(1000)]
	public string? Skills { get; set; }

	[MaxLength(1000)]
	public string? Gender { get; set; }

	[MaxLength(1000)]
	public string? AgeCategories { get; set; }

	[MaxLength(1000)]
	public string? States { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	[MaxLength(1000)]
	public string? Users { get; set; }

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
	public GroupChatEntity? GroupChat { get; set; }
	public Guid? GroupChatId { get; set; }
	public CategoryEntity? Category { get; set; }
    public Guid? CategoryId { get; set; }
    public UserEntity? UserPromoted { get; set; }
    public string? UserPromotedId { get; set; }
}

public class CreateUpdatePromotionDto {
	public Guid? Id { get; set; }
	public DisplayType DisplayType { get; set; }
	public List<string>? Skills { get; set; }
	public List<string>? Gender { get; set; }
	public List<string>? AgeCategories { get; set; }
	public List<string>? States { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? GroupChatId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? UserId { get; set; }
}

public class PromotionDetail {
	public int TotalSeen { get; set; }
	public List<KeyValue> StatePerUsers { get; set; } = null!;
	public List<KeyValue> SkillPerUsers { get; set; } = null!;
	public List<KeyValue> AgeCategoryPerUsers { get; set; } = null!;
}