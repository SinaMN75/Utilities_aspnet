namespace Utilities_aspnet.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity {
	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? SubTitle { get; set; }

	[StringLength(2000)]
	public string? Description { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[StringLength(500)]
	public string? Type { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class ContentCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
}

public class ContentReadDto {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
}

public static class ContentExtension {
	public static ContentReadDto Map(this ContentEntity e) => new() {
		Description = e.Description,
		Title = e.Title,
		Type = e.Type,
		SubTitle = e.SubTitle,
		UseCase = e.UseCase
	};
	
	public static ContentEntity Map(this ContentCreateUpdateDto e) => new() {
		Description = e.Description,
		Title = e.Title,
		Type = e.Type,
		SubTitle = e.SubTitle,
		UseCase = e.UseCase
	};
}