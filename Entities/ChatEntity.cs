namespace Utilities_aspnet.Entities;

[Table("GroupChat")]
public class GroupChatEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(50)]
	public string? CreatorUserId { get; set; }

	public ChatType? Type { get; set; }

	public GroupChatJsonDetail JsonDetail { get; set; } = new();

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<UserEntity>? Users { get; set; }
	public IEnumerable<ProductEntity>? Products { get; set; }
	public IEnumerable<GroupChatMessageEntity>? GroupChatMessage { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }
	public IEnumerable<NotificationEntity?>? Notifications { get; set; }

	[NotMapped]
	public int CountOfUnreadMessages { get; set; }
}

public class GroupChatJsonDetail {
	public string? Description { get; set; }
	public string? Value { get; set; }
	public string? Department { get; set; }
	public ChatStatus? ChatStatus { get; set; }
	public Priority? Priority { get; set; }
	public DateTime? Boosted { get; set; }
}

[Table("GroupChatMessage")]
public class GroupChatMessageEntity : BaseEntity {
	[MaxLength(2000)]
	public string? Message { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public GroupChatEntity? GroupChat { get; set; }

	public Guid? GroupChatId { get; set; }

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	[ForeignKey(nameof(ForwardedMessageId))]
	public GroupChatMessageEntity? ForwardedMessage { get; set; }

	public Guid? ForwardedMessageId { get; set; }

	[ForeignKey(nameof(ParentId))]
	public GroupChatMessageEntity? Parent { get; set; }

	public Guid? ParentId { get; set; }

	public SeenUsers? SeenUsers { get; set; }
	public Guid? SeenUsersId { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<ProductEntity?>? Products { get; set; }

	[NotMapped]
	public List<UserEntity>? MessageSeenBy { get; set; }
}

[Table("SeenUsers")]
public class SeenUsers : BaseEntity {
	public Guid? FkGroupChat { get; set; }

	[MaxLength(50)]
	public string? FkUserId { get; set; }

	public Guid? FkGroupChatMessage { get; set; }
}

public class GroupChatFilterDto : BaseFilterDto {
	public IEnumerable<Guid>? Ids { get; set; }
	public IEnumerable<string>? UsersIds { get; set; }
	public IEnumerable<Guid>? ProductsIds { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Value { get; set; }
	public string? Department { get; set; }
	public ChatStatus? ChatStatus { get; set; }
	public ChatType? Type { get; set; }
	public Priority? Priority { get; set; }
	public bool? ShowUsers { get; set; }
	public bool? ShowProducts { get; set; }
	public bool? ShowCategories { get; set; }
	public bool ShowAhtorized { get; set; }
	public bool? OrderByAtoZ { get; set; } = false;
	public bool? OrderByZtoA { get; set; } = false;
	public bool? OrderByCreatedDate { get; set; } = false;
	public bool? OrderByCreaedDateDecending { get; set; } = false;
	public bool Boosted { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}

public class GroupChatCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Value { get; set; }
	public string? Department { get; set; }
	public ChatStatus? ChatStatus { get; set; }
	public ChatType? Type { get; set; }
	public Priority? Priority { get; set; }
	public IEnumerable<string>? UserIds { get; set; } = new List<string>();
	public IEnumerable<Guid>? Products { get; set; } = new List<Guid>();
	public IEnumerable<Guid>? Categories { get; set; } = new List<Guid>();
}

public class GroupChatMessageCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Message { get; set; }
	public Guid? ForwardedMessageId { get; set; }
	public Guid? GroupChatId { get; set; }
	public Guid? ParentId { get; set; }
	public IEnumerable<Guid>? Products { get; set; } = new List<Guid>();
}

public class FilterGroupChatMessagesDto: BaseFilterDto {
	public Guid? GroupChatId { get; set; }
	public string? Message { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}