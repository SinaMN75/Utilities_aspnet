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

	[StringLength(500)]
	public string? Time { get; set; }

	[StringLength(500)]
	public string? Artist { get; set; }

	[StringLength(500)]
	public string? Album { get; set; }
	
	public bool? IsPrivate { get; set; }

	[NotMapped]
	public string Url => $"{Server.ServerAddress}/Medias/{FileName}";

	[SwaggerIgnore]
	public ContentEntity? Content { get; set; }

	[SwaggerIgnore]
	public Guid? ContentId { get; set; }

	[SwaggerIgnore]
	public UserEntity? User { get; set; }

	[SwaggerIgnore]
	public string? UserId { get; set; }

	[SwaggerIgnore]
	public ProductEntity? Product { get; set; }

	[SwaggerIgnore]
	public Guid? ProductId { get; set; }

	[SwaggerIgnore]
	public CommentEntity? Comment { get; set; }

	[SwaggerIgnore]
	public Guid? CommentId { get; set; }

	[SwaggerIgnore]
	public ChatEntity? Chat { get; set; }

	[SwaggerIgnore]
	public Guid? ChatId { get; set; }

	[SwaggerIgnore]
	public NotificationEntity? Notification { get; set; }

	[SwaggerIgnore]
	public Guid? NotificationId { get; set; }

	[SwaggerIgnore]
	public CategoryEntity? Category { get; set; }

	[SwaggerIgnore]
	public Guid? CategoryId { get; set; }

	[SwaggerIgnore]
	public GroupChatEntity? GroupChat { get; set; }

	[SwaggerIgnore]
	public Guid? GroupChatId { get; set; }

	[SwaggerIgnore]
	public GroupChatMessageEntity? GroupChatMessage { get; set; }

	[SwaggerIgnore]
	public Guid? GroupChatMessageId { get; set; }

	[SwaggerIgnore]
	public Guid? BookmarkId { get; set; }

	[SwaggerIgnore]
	public BookmarkEntity? Bookmark { get; set; }
}

public class UploadDto {
	public string? UseCase { get; set; }
	public string? UserId { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public bool? IsPrivate { get; set; }
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
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
}