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

	public IQueryable<MediaEntity>? Media { get; set; }
}

public class ContentReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
}

public static class ContentEntityExtension {
	public static IQueryable<ContentReadDto> MapContentReadDto(this IQueryable<ContentEntity> e) {
		return e.Select(x => new ContentReadDto {
			Id = x.Id,
			Title = x.Title,
			Type = x.Type,
			UseCase = x.UseCase,
			Media = x.Media!.MapContentReadDto()
		});
	}
}