namespace Utilities_aspnet.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(100)]
	public string? SubTitle { get; set; }

	[MaxLength(5000)]
	public string? Description { get; set; }

	[MaxLength(100)]
	public List<TagContent>? Tags { get; set; } = new();

	[MaxLength(1000)]
	public ContentJsonDetail JsonDetail { get; set; } = new();

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class ContentJsonDetail {
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? PhoneNumber1 { get; set; }
	public string? PhoneNumber2 { get; set; }
	public string? Email1 { get; set; }
	public string? Email2 { get; set; }
	public string? Address1 { get; set; }
	public string? Address2 { get; set; }
	public string? Address3 { get; set; }
}

public class ContentReadDto {
	public Guid Id { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public List<TagContent>? Tags { get; set; }
	public ContentJsonDetail? JsonDetail { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
}

public class ContentCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? PhoneNumber1 { get; set; }
	public string? PhoneNumber2 { get; set; }
	public string? Email1 { get; set; }
	public string? Email2 { get; set; }
	public string? Address1 { get; set; }
	public string? Address2 { get; set; }
	public string? Address3 { get; set; }
	public List<TagContent>? Tags { get; set; }
	public List<TagContent>? RemoveTags { get; set; }
	public List<TagContent>? AddTags { get; set; }
}

public class ContentCreateDto {
	public string Title { get; set; } = "";
	public string SubTitle { get; set; } = "";
	public string Description { get; set; } = "";
	public string Instagram { get; set; } = "";
	public string Telegram { get; set; } = "";
	public string WhatsApp { get; set; } = "";
	public string LinkedIn { get; set; } = "";
	public string Dribble { get; set; } = "";
	public string SoundCloud { get; set; } = "";
	public string Pinterest { get; set; } = "";
	public string Website { get; set; } = "";
	public string PhoneNumber1 { get; set; } = "";
	public string PhoneNumber2 { get; set; } = "";
	public string Email1 { get; set; } = "";
	public string Email2 { get; set; } = "";
	public string Address1 { get; set; } = "";
	public string Address2 { get; set; } = "";
	public string Address3 { get; set; } = "";
	public List<TagContent> Tags { get; set; } = new();
}

public class ContentUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? PhoneNumber1 { get; set; }
	public string? PhoneNumber2 { get; set; }
	public string? Email1 { get; set; }
	public string? Email2 { get; set; }
	public string? Address1 { get; set; }
	public string? Address2 { get; set; }
	public string? Address3 { get; set; }
	public List<TagContent>? Tags { get; set; }
	public List<TagContent>? RemoveTags { get; set; }
	public List<TagContent>? AddTags { get; set; }
}

public static class ContentEntityEx {
	public static ContentReadDto MapReadDto(this ContentEntity e) {
		return new ContentReadDto {
			Id = e.Id,
			Title = e.Title,
			SubTitle = e.SubTitle,
			Description = e.Description,
			Tags = e.Tags,
			JsonDetail = e.JsonDetail,
			Media = e.Media?.Select(y => new MediaReadDto { Id = y.Id, FileName = y.FileName, Order = y.Order, JsonDetail = y.JsonDetail, Tags = y.Tags })
		};
	}

	public static IQueryable<ContentReadDto> MapReadDto(this IQueryable<ContentEntity> e) {
		return e.Select(x => new ContentReadDto {
			Id = x.Id,
			Title = x.Title,
			SubTitle = x.SubTitle,
			Description = x.Description,
			Tags = x.Tags,
			JsonDetail = x.JsonDetail,
			Media = x.Media!.Select(y => new MediaReadDto { Id = y.Id, FileName = y.FileName, Order = y.Order, JsonDetail = y.JsonDetail, Tags = y.Tags })
		});
	}
}