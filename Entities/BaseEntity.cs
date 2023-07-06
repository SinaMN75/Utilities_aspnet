namespace Utilities_aspnet.Entities;

public class BaseEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public class KeyValue {
	public string? Key { get; set; }
	public string? Value { get; set; }
}