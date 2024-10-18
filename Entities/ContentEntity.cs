namespace Utilities_aspnet.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity {
	[MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(100)]
	public string? SubTitle { get; set; }

	[MaxLength(5000)]
	public required string Description { get; set; }

	[MaxLength(100)]
	public required List<TagContent> Tags { get; set; } = [];

	public required ContentJsonDetail JsonDetail { get; set; } = new();

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
	public long? Price { get; set; }
	public int? Days { get; set; }
	public List<KeyValue> KeyValues { get; set; } = [];
}

public class ContentCreateDto {
	public required string Title { get; set; }
	public required string Description { get; set; }
	public string? SubTitle { get; set; }
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
	public long? Price { get; set; }
	public int? Days { get; set; }
	public required List<TagContent> Tags { get; set; }
	public List<KeyValue> KeyValues { get; set; } = [];
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
	public long? Price { get; set; }
	public int? Days { get; set; }
	public List<TagContent>? Tags { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
}