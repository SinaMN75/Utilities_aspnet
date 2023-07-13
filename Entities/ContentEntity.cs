namespace Utilities_aspnet.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(100)]
	public string? SubTitle { get; set; }

	[StringLength(5000)]
	public string? Description { get; set; }

	[MaxLength(100)]
	public List<TagContent>? Tags { get; set; } = new();

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class ContentCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public List<TagContent>? Tags { get; set; }
}