namespace Utilities_aspnet.Entities;

[Table("Notifications")]
public class NotificationEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(500)]
	public string? Message { get; set; }

	[MaxLength(500)]
	public string? Link { get; set; }

	[MaxLength(100)]
	public List<TagNotification> Tags { get; set; } = [];

	public SeenStatus? SeenStatus { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public string? UserId { get; set; }

	[ForeignKey(nameof(UserId))]
	public UserEntity? User { get; set; }

	public string? CreatorUserId { get; set; }

	[ForeignKey(nameof(CreatorUserId))]
	public UserEntity? CreatorUser { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public GroupChatEntity? GroupChat;
	public Guid? GroupChatId;
	
	public CommentEntity? Comment;
	public Guid? CommentId;
}

public class NotificationCreateUpdateDto {
	public string? Title { get; set; }
	public string? UserId { get; set; }
	public string? CreatorUserId { get; set; }
	public string? Message { get; set; }
	public string? Link { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? GroupChatId { get; set; }
	public Guid? CommentId { get; set; }
	public List<TagNotification>? Tags { get; set; }
	public List<TagNotification>? RemoveTags { get; set; }
	public List<TagNotification>? AddTags { get; set; }
}

public class NotificationFilterDto : BaseFilterDto {
	public string? Title { get; set; }
	public string? UserId { get; set; }
	public string? CreatorUserId { get; set; }
	public string? Message { get; set; }
	public bool? ShowMedia { get; set; }
	public bool? ShowProduct { get; set; }
	public bool? ShowComment { get; set; }
	public bool? ShowCreator { get; set; }
	public bool? ShowUser { get; set; }
	public bool? ShowChatMessage { get; set; }
	public bool? ShowGroupChat { get; set; }
	public List<TagNotification>? Tags { get; set; }
}