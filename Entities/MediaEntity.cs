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
	public Guid? GroupChatId { get; set; }

	[JsonIgnore]
	public Guid? GroupChatMessageId { get; set; }

	[JsonIgnore]
	public Guid? BookmarkId { get; set; }
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
}

public class UploadDto {
	public Guid? Id { get; set; }
	public int? Order { get; set; }
	public string? UserId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
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
	public int? Order { get; set; }
	public string? Link1 { get; set; }
	public string? Link2 { get; set; }
	public string? Link3 { get; set; }
	public List<TagMedia>? Tags { get; set; }
}

public class MediaFilterDto : BaseFilterDto;