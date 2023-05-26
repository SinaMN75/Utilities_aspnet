namespace Utilities_aspnet.Entities;

[Table("Chats")]
public class ChatEntity : BaseEntity {
	[ForeignKey(nameof(FromUser))]
	public string FromUserId { get; set; } = null!;

	public UserEntity FromUser { get; set; } = null!;

	[ForeignKey(nameof(ToUser))]
	public string ToUserId { get; set; } = null!;

	public UserEntity ToUser { get; set; } = null!;

	[StringLength(2000)]
	public string MessageText { get; set; } = null!;

	public bool ReadMessage { get; set; }

	public Guid? ParentId { get; set; }
	public ChatEntity? Parent { get; set; }

	public IEnumerable<ReactionEntity>? ChatReacts { get; set; }
	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<ProductEntity?>? Products { get; set; }
}

[Table("GroupChat")]
public class GroupChatEntity : BaseEntity {
	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? CreatorUserId { get; set; }

	public ChatType? Type { get; set; }

	public GroupChatJsonDetail? GroupChatJsonDetail { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<UserEntity>? Users { get; set; }
	public IEnumerable<ProductEntity>? Products { get; set; }
	public IEnumerable<GroupChatMessageEntity>? GroupChatMessage { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }

	[NotMapped]
	public int CountOfUnreadMessages { get; set; }
}

public class GroupChatJsonDetail {
	public string? Description { get; set; }
	public string? Value { get; set; }
	public string? Department { get; set; }
	public ChatStatus? ChatStatus { get; set; }
	public Priority? Priority { get; set; }
	public bool? IsBoosted { get; set; }
}

[Table("GroupChatMessage")]
public class GroupChatMessageEntity : BaseEntity {
	[StringLength(2000)]
	public string? Message { get; set; }

	[StringLength(500)]
	public string? Type { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

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
	public Guid? Fk_GroupChat { get; set; }
	public string? Fk_UserId { get; set; }
	public Guid? Fk_GroupChatMessage { get; set; }
}

public class ChatReadDto {
	public Guid? Id { get; set; }
	public string? UserId { get; set; } = null!;
	public string? MessageText { get; set; }
	public DateTime? DateTime { get; set; }
	public bool? Send { get; set; }
	public int? UnReadMessages { get; set; } = 0;
	public Guid? ParentId { get; set; }
	public IEnumerable<MediaEntity>? Media { get; set; }
	public UserEntity? User { get; set; }
	public IEnumerable<ProductEntity?>? Products { get; set; }
	public ChatReadDto? Parent { get; set; }
}

public class ChatFilterDto {
	public string UserId { get; set; } = null!;
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
}

public class GroupChatFilterDto {
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
	public bool IsBoosted { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
}

public class ChatCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? UserId { get; set; }
	public string? MessageText { get; set; }
	public IEnumerable<Guid>? Products { get; set; }
	public IEnumerable<string>? Users { get; set; }
	public Guid? ParentId { get; set; }
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
	public string? Type { get; set; }
	public string? UseCase { get; set; }
	public Guid? ForwardedMessageId { get; set; }
	public Guid? GroupChatId { get; set; }
	public Guid? ParentId { get; set; }
	public IEnumerable<Guid>? Products { get; set; } = new List<Guid>();
}