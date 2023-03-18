namespace Utilities_aspnet.Entities;

[Table("Media")]
public class MediaEntity : BaseEntity {
	[StringLength(500)]
	public string? FileName { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[StringLength(500)]
	public string? Link { get; set; }

	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? Size { get; set; }

	[NotMapped]
	public string Url => $"{Server.ServerAddress}/Medias/{FileName}";

	[JsonIgnore]
	public ContentEntity? Content { get; set; }

	[JsonIgnore]
	public Guid? ContentId { get; set; }

	[JsonIgnore]
	public UserEntity? User { get; set; }

	[JsonIgnore]
	public string? UserId { get; set; }

	[JsonIgnore]
	public ProductEntity? Product { get; set; }

	[JsonIgnore]
	public Guid? ProductId { get; set; }

	[JsonIgnore]
	public CommentEntity? Comment { get; set; }

	[JsonIgnore]
	public Guid? CommentId { get; set; }

	[JsonIgnore]
	public ChatEntity? Chat { get; set; }

	[JsonIgnore]
	public Guid? ChatId { get; set; }

	[JsonIgnore]
	public NotificationEntity? Notification { get; set; }

	[JsonIgnore]
	public Guid? NotificationId { get; set; }

	[JsonIgnore]
	public CategoryEntity? Category { get; set; }

	[JsonIgnore]
	public Guid? CategoryId { get; set; }

	[JsonIgnore]
	public GroupChatEntity? GroupChat { get; set; }

	[JsonIgnore]
	public Guid? GroupChatId { get; set; }

	[JsonIgnore]
	public GroupChatMessageEntity? GroupChatMessage { get; set; }

	[JsonIgnore]
	public Guid? GroupChatMessageId { get; set; }

	[JsonIgnore]
	public Guid? BookmarkId { get; set; }

	[JsonIgnore]
	public BookmarkEntity? Bookmark { get; set; }
}

public class UploadDto {
	public string? UseCase { get; set; }
	public string? UserId { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
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
}

public class UpdateMediaDto {
	public string? UseCase { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
}