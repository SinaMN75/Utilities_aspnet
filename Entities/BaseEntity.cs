namespace Utilities_aspnet.Entities;

public class BaseEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	
	[System.Text.Json.Serialization.JsonIgnore]
	public DateTime? DeletedAt { get; set; }
}