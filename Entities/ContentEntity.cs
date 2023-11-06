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
