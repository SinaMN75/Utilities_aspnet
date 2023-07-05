namespace Utilities_aspnet.Entities;

public class BaseEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public DateTime? DeletedAt { get; set; }
}

public class KeyValue {
	public string? Key { get; set; }
	public string? Value { get; set; }
}

public class ProductCategory {
	public CategoryEntity? Category { get; set; }
	public ProductEntity? Product { get; set; }

	public Guid? ProductId { get; set; }
	public Guid? CategoryId { get; set; }
}