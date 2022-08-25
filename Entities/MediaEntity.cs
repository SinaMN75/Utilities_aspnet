﻿namespace Utilities_aspnet.Entities;

[Table("Media")]
public class MediaEntity : BaseEntity {
	[MaxLength(500)]
	public string? FileName { get; set; }

	[MaxLength(500)]
	public string? UseCase { get; set; }

	[MaxLength(500)]
	public string? Link { get; set; }

	[MaxLength(500)]
	public string? Title { get; set; }

	[MaxLength(500)]
	public string? Size { get; set; }

	public VisibilityType? Visibility { get; set; } = VisibilityType.Public;

	[NotMapped]
	public string Url => $"{Server.ServerAddress}/Medias/{FileName}";

	[System.Text.Json.Serialization.JsonIgnore]
	public ContentEntity? Content { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Guid? ContentId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public UserEntity? User { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public string? UserId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public ProductEntity? Product { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Guid? ProductId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public CommentEntity? Comment { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Guid? CommentId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public ChatEntity? Chat { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Guid? ChatId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public NotificationEntity? Notification { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Guid? NotificationId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public CategoryEntity? Category { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Guid? CategoryId { get; set; }
}

public class UploadDto {
	public string? UseCase { get; set; }
	public string? UserId { get; set; }
	public IEnumerable<IFormFile>? Files { get; set; }
	public IEnumerable<string>? Links { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
	public VisibilityType? Visibility { get; set; } = VisibilityType.Public;
	public Guid? ProductId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? NotificationId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? ChatId { get; set; }
}