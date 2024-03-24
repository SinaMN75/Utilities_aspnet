namespace Utilities_aspnet.Entities;

[Table("Media")]
public class MediaEntity : BaseEntity {
	[MaxLength(100)]
	public string? FileName { get; set; }

	public int? Order { get; set; }

	public MediaJsonDetail JsonDetail { get; set; } = new();

	[MaxLength(100)]
	public List<TagMedia> Tags { get; set; } = [];

	public Guid? ParentId { get; set; }
	public MediaEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<MediaEntity>? Children { get; set; }

	[NotMapped]
	public string Url => $"{Server.ServerAddress}/Medias/{FileName}";

	// [NotMapped]
	// public string S3Url {
	// 	get {
	// 		try {
	// 			AmazonS3Settings amazonS3Settings = AppSettings.GetCurrentSettings().AmazonS3Settings;
	// 			if (!(amazonS3Settings.UseS3 ?? false)) return "";
	// 			GetPreSignedUrlRequest getPreSignedUrlRequest = new() {
	// 				BucketName = amazonS3Settings.DefaultBucket,
	// 				Key = FileName,
	// 				Expires = DateTime.UtcNow.AddDays(1),
	// 				Verb = HttpVerb.GET
	// 			};
	// 			return new AmazonS3Client(
	// 				new BasicAWSCredentials(amazonS3Settings.AccessKey, amazonS3Settings.SecretKey),
	// 				new AmazonS3Config { ServiceURL = amazonS3Settings.Url }).GetPreSignedURL(getPreSignedUrlRequest);
	// 		}
	// 		catch {
	// 			return "";
	// 		}
	// 	}
	// }

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
	public string? Description { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public string? Link1 { get; set; }
	public string? Link2 { get; set; }
	public string? Link3 { get; set; }
	public PrivacyType? IsPrivate { get; set; }
}

public class UploadDto {
	public Guid? Id { get; set; }
	public int? Order { get; set; }
	public string? UserId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
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
	public Guid? ParentId { get; set; }
	public IEnumerable<string>? Links { get; set; }
	public IFormFile? File { get; set; }
	public List<TagMedia> Tags { get; set; } = [];
	public string? Link1 { get; set; }
	public string? Link2 { get; set; }
	public string? Link3 { get; set; }
}

public class UpdateMediaDto {
	public Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public int? Order { get; set; }
	public string? Link1 { get; set; }
	public string? Link2 { get; set; }
	public string? Link3 { get; set; }
	public List<TagMedia>? Tags { get; set; }
	public List<TagMedia>? RemoveTags { get; set; }
	public List<TagMedia>? AddTags { get; set; }
}

public class MediaFilterDto : BaseFilterDto;