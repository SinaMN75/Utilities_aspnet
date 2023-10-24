namespace Utilities_aspnet.Entities;

public class BaseEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.Now;
	public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class KeyValue {
	public string? Key { get; set; }
	public string? Value { get; set; }
}

public class BaseFilterDto {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
}