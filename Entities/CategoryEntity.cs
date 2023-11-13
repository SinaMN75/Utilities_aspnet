namespace Utilities_aspnet.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(100)]
	public string? TitleTr1 { get; set; }

	[MaxLength(100)]
	public string? TitleTr2 { get; set; }

	[MaxLength(20)]
	public string? UseCase { get; set; }

	[MaxLength(20)]
	public string? Type { get; set; }

	public int? Order { get; set; }

	public CategoryJsonDetail JsonDetail { get; set; } = new();

	[MaxLength(100)]
	public List<TagCategory> Tags { get; set; } = new();

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public IEnumerable<UserEntity>? Users { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public IEnumerable<ProductEntity>? Products { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public IEnumerable<GroupChatEntity>? GroupChats { get; set; }
}

public class CategoryJsonDetail {
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? Value { get; set; }
	public long? DiscountedPrice { get; set; }
	public long? SendPrice { get; set; }
	public long? Price { get; set; }
	public int? Stock { get; set; }
	public string? Color { get; set; }
	public DateTime? Boosted { get; set; }
	public DateTime? Date1 { get; set; }
	public DateTime? Date2 { get; set; }
}

public class CategoryCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? Subtitle { get; set; }
	public string? Color { get; set; }
	public string? Link { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public long? Price { get; set; }
	public long? DiscountedPrice { get; set; }
	public long? SendPrice { get; set; }
	public int? Value { get; set; }
	public int? Stock { get; set; }
	public int? Order { get; set; }
	public DateTime? Date1 { get; set; }
	public DateTime? Date2 { get; set; }
	public Guid? ParentId { get; set; }
	public bool IsUnique { get; set; } = true;
	public List<TagCategory>? Tags { get; set; }
	public List<TagCategory>? RemoveTags { get; set; }
	public List<TagCategory>? AddTags { get; set; }
}

public class CategoryFilterDto : BaseFilterDto {
	public string? Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public List<TagCategory>? Tags { get; set; }
	public Guid? ParentId { get; set; }
	public bool? ShowByChildren { get; set; }
	public bool? ShowMedia { get; set; }
	public bool? OrderByOrder { get; set; }
	public bool? OrderByOrderDescending { get; set; }
	public bool? OrderByCreatedAt { get; set; }
	public bool? OrderByCreatedAtDescending { get; set; }
}