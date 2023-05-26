namespace Utilities_aspnet.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity {
	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? TitleTr1 { get; set; }

	[StringLength(500)]
	public string? TitleTr2 { get; set; }

	[StringLength(500)]
	public string? Color { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[StringLength(500)]
	public string? Type { get; set; }

	public double? Price { get; set; }
	public int? Stock { get; set; }
	public int? Order { get; set; }

	public CategoryJsonDetail CategoryJsonDetail { get; set; } = new();

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
	public IEnumerable<FormEntity>? FormFields { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public IEnumerable<OrderDetailEntity>? OrderDetails { get; set; }

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
	public double? Price { get; set; }
	public double? Value { get; set; }
	public int? Stock { get; set; }
	public int? Order { get; set; }
	public DateTime? Date1 { get; set; }
	public DateTime? Date2 { get; set; }
	public Guid? ParentId { get; set; }
	public bool IsUnique { get; set; } = true;
}

public class CategoryFilterDto {
	public string? Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public Guid? ParentId { get; set; }
	public bool? OrderByOrder { get; set; }
	public bool? OrderByOrderDecending { get; set; }
	public bool? OrderByCreatedAt { get; set; }
	public bool? OrderByCreatedAtDecending { get; set; }
}