namespace Utilities_aspnet.Entities;

[Table("Media")]
public class MediaEntity : BaseEntity {
	[MaxLength(50)]
	public string? FileName { get; set; }

	[MaxLength(20)]
	public string? UseCase { get; set; }

	public int? Order { get; set; }

	[MaxLength(1000)]
	public MediaJsonDetail JsonDetail { get; set; } = new();

	[MaxLength(100)]
	public List<TagMedia>? Tags { get; set; } = new();

	[NotMapped]
	public string Url => $"{Server.ServerAddress}/Medias/{FileName}";

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public ContentEntity? Content { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? ContentId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public UserEntity? User { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string? UserId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public ProductEntity? Product { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? ProductId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public CommentEntity? Comment { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? CommentId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? ChatId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public NotificationEntity? Notification { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? NotificationId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public CategoryEntity? Category { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? CategoryId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public GroupChatEntity? GroupChat { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? GroupChatId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public GroupChatMessageEntity? GroupChatMessage { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? GroupChatMessageId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? BookmarkId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public BookmarkEntity? Bookmark { get; set; }
}

public class MediaJsonDetail {
	public string? Title { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public PrivacyType? IsPrivate { get; set; }
}

public class UploadDto {
	public int? Order { get; set; }
	public string? UseCase { get; set; }
	public string? UserId { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public PrivacyType? PrivacyType { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? NotificationId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? ChatId { get; set; }
	public Guid? GroupChatId { get; set; }
	public Guid? GroupChatMessageId { get; set; }
	public Guid? BookmarkId { get; set; }
	public IEnumerable<string>? Links { get; set; }
	public IEnumerable<IFormFile>? Files { get; set; }
	public List<TagMedia>? Tags { get; set; }
}

public class UpdateMediaDto {
	public string? UseCase { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public int? Order { get; set; }
	public List<TagMedia>? Tags { get; set; }
}