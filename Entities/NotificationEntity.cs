namespace Utilities_aspnet.Entities;

[Table("Notifications")]
public class NotificationEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(500)]
	public string? Message { get; set; }

	[MaxLength(500)]
	public string? Link { get; set; }

	[MaxLength(20)]
	public string? UseCase { get; set; }

	[MaxLength(100)]
	public List<TagNotification>? Tags { get; set; } = new();

	public SeenStatus? SeenStatus { get; set; }
	public bool? Visited { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public string? UserId { get; set; }

	[ForeignKey(nameof(UserId))]
	public UserEntity? User { get; set; }

	public string? CreatorUserId { get; set; }

	[ForeignKey(nameof(CreatorUserId))]
	public UserEntity? CreatorUser { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }
}

public class NotificationCreateUpdateDto {
	public string? Title { get; set; }
	public string? UserId { get; set; }
	public string? CreatorUserId { get; set; }
	public string? Message { get; set; }
	public string? Link { get; set; }
	public string? UseCase { get; set; }
	public Guid? ProductId { get; set; }
	public List<TagNotification>? Tags { get; set; }
	public List<TagNotification>? RemoveTags { get; set; }
	public List<TagNotification>? AddTags { get; set; }
}

public class NotificationFilterDto {
	public string? Title { get; set; }
	public string? UserId { get; set; }
	public string? CreatorUserId { get; set; }
	public string? Message { get; set; }
	public string? UseCase { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public List<TagNotification>? Tags { get; set; }
}