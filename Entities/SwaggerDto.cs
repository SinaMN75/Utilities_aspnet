namespace Utilities_aspnet.Entities;

public class GroupChatReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Value { get; set; }
	public string? Department { get; set; }
	public string? CreatorUserId { get; set; }
	public int CountOfUnreadMessages { get; set; }
	public ChatStatus? ChatStatus { get; set; }
	public ChatType? Type { get; set; }
	public Priority? Priority { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
	public IEnumerable<UserReadDto>? Users { get; set; }
	public IEnumerable<ProductReadDto>? Products { get; set; }
	public IEnumerable<GroupChatReadDto>? GroupChatMessage { get; set; }
	public IEnumerable<CategoryReadDto>? Categories { get; set; }
}

public class CategoryReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? Subtitle { get; set; }
	public string? Color { get; set; }
	public string? Link { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? Price { get; set; }
	public double? Value { get; set; }
	public double? Stock { get; set; }
	public DateTime? Date1 { get; set; }
	public DateTime? Date2 { get; set; }
	public IEnumerable<CategoryReadDto>? Children { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
}

public class GroupChatMessageReadDto : BaseEntity {
	public string? Message { get; set; }
	public string? Type { get; set; }
	public string? UseCase { get; set; }
	public string? UserId { get; set; }
	public GroupChatReadDto? GroupChat { get; set; }
	public UserReadDto? User { get; set; }
	public GroupChatMessageReadDto? ForwardedMessage { get; set; }
	public GroupChatMessageReadDto? Parent { get; set; }
	public SeenUsers? SeenUsers { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
	public IEnumerable<ProductReadDto?>? Products { get; set; }
	public IEnumerable<UserReadDto>? MessageSeenBy { get; set; }
}

public class CommentReadDto : BaseEntity {
	public double? Score { get; set; } = 0;
	public string? Comment { get; set; }
	public ChatStatus? Status { get; set; }
	public UserReadDto? User { get; set; }
	public ProductReadDto? Product { get; set; }
	public IEnumerable<CommentReadDto>? Children { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
	public IEnumerable<LikeCommentReadDto>? LikeComments { get; set; }
	public IEnumerable<CommentReacts>? CommentReacts { get; set; }
	public bool IsLiked { get; set; }
}

public class LikeCommentReadDto : BaseEntity {
	public double? Score { get; set; } = 0;
	public UserReadDto? User { get; set; }
	public string? UserId { get; set; }
}

public class ContentReadDto {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
}

public class DiscountReadDto : BaseEntity {
	public string? Title { get; set; }
	public int? DiscountPercent { get; set; }
	public int? NumberUses { get; set; }
	public string? Code { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}

public class FormReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? UseCase { get; set; }
	public FormFieldReadDto? FormField { get; set; }
	public Guid? FormFieldId { get; set; }
}

public class FormFieldReadDto : BaseEntity {
	public string? Label { get; set; }
	public string? OptionList { get; set; }
	public string? UseCase { get; set; }
	public bool? IsRequired { get; set; } = false;
	public FormFieldType? Type { get; set; }
}

public class MediaReadDto : BaseEntity {
	public string? FileName { get; set; }
	public string? UseCase { get; set; }
	public string? Link { get; set; }
	public string? Title { get; set; }
	public string? Size { get; set; }
	public string? Time { get; set; }
	public string? Artist { get; set; }
	public string? Album { get; set; }
	public string Url => $"{Server.ServerAddress}/Medias/{FileName}";
}

public class NotificationReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? Message { get; set; }
	public string? Link { get; set; }
	public string? UseCase { get; set; }
	public SeenStatus? SeenStatus { get; set; }
	public bool? Visited { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
	public UserReadDto? User { get; set; }
	public UserReadDto? CreatorUser { get; set; }
	public ProductReadDto? Product { get; set; }
	public Guid? ProductId { get; set; }
}

public class OrderReadDto : BaseEntity {
	public string? Description { get; set; }
	public string? DiscountCode { get; set; }
	public string? ProductUseCase { get; set; }
	public string? PayNumber { get; set; }
	public OrderStatuses? Status { get; set; }
	public double? TotalPrice { get; set; }
	public double? DiscountPrice { get; set; }
	public int? DiscountPercent { get; set; }
	public double? SendPrice { get; set; }
	public SendType? SendType { get; set; }
	public PayType? PayType { get; set; }
	public DateTime? PayDateTime { get; set; }
	public DateTime? ReceivedDate { get; set; }
	public UserReadDto? User { get; set; }
	public string? UserId { get; set; }
	public UserReadDto? ProductOwner { get; set; }
	public string? ProductOwnerId { get; set; }
	public IEnumerable<OrderDetailReadDto>? OrderDetails { get; set; }
	public string? State { get; set; }
}

public class OrderDetailReadDto : BaseEntity {
	public double? Price { get; set; }
	public int? Count { get; set; }
	public OrderReadDto? Order { get; set; }
	public ProductReadDto? Product { get; set; }
	public CategoryReadDto? Category { get; set; }
}

public class ProductReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Details { get; set; }
	public string? Address { get; set; }
	public string? Author { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Link { get; set; }
	public string? Website { get; set; }
	public string? Email { get; set; }
	public string? Type { get; set; }
	public string? UseCase { get; set; }
	public string? Unit { get; set; }
	public string? State { get; set; }
	public string? StateTr1 { get; set; }
	public string? StateTr2 { get; set; }
	public string? KeyValues1 { get; set; }
	public string? KeyValues2 { get; set; }
	public string? RelatedIds { get; set; }
	public string Teams { get; set; } = "";
	public double? Latitude { get; set; }
	public double? ResponseTime { get; set; }
	public double? OnTimeDelivery { get; set; }
	public double? Longitude { get; set; }
	public double? Length { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public double? Weight { get; set; }
	public double? MinOrder { get; set; }
	public double? MaxOrder { get; set; }
	public double? MaxPrice { get; set; }
	public double? MinPrice { get; set; }
	public double? Price { get; set; }
	public double? VoteCount { get; set; }
	public double? DiscountPrice { get; set; }
	public int? DiscountPercent { get; set; }
	public int? VisitsCount { get; set; }
	public bool? Enabled { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public ProductStatus? Status { get; set; }
	public Currency? Currency { get; set; }
	public double? Stock { get; set; }
	public string? UserId { get; set; }
	public UserReadDto? User { get; set; }
	public DateTime? ExpireDate { get; set; }
	public string? SeenUsers { get; set; }
	public IEnumerable<MediaReadDto>? Media { get; set; }
	public IEnumerable<FormReadDto>? Forms { get; set; }
	public IEnumerable<CategoryReadDto>? Categories { get; set; }
	public IEnumerable<CommentReadDto>? Comments { get; set; }
	public IEnumerable<ProductInsightReadDto>? ProductInsights { get; set; }
	public IEnumerable<VisitProductsReadDto>? VisitProducts { get; set; }
}

public class ProductInsightReadDto : BaseEntity {
	public ChatReaction? Reaction { get; set; }
	public UserReadDto? User { get; set; }
	public ProductReadDto? Product { get; set; }
	public int? Count { get; set; } = 0;
}

public class VisitProductsReadDto : BaseEntity {
	public UserReadDto? User { get; set; }
	public ProductReadDto? Product { get; set; }
}

public class ReportReadDto : BaseEntity {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public UserReadDto? CreatorUser { get; set; }
	public UserReadDto? User { get; set; }
	public ProductReadDto? Product { get; set; }
	public CommentReadDto? Comment { get; set; }
	public ChatReadDto? Chat { get; set; }
	public GroupChatMessageReadDto? GroupChatMessage { get; set; }
	public GroupChatReadDto? GroupChat { get; set; }
	public ReportType ReportType { get; set; } = ReportType.All;
}

public class TransactionReadDto : BaseEntity {
	public double? Amount { get; set; }
	public string? Descriptions { get; set; }
	public string? Authority { get; set; }
	public string? GatewayName { get; set; }
	public string? PaymentId { get; set; }
	public long? RefId { get; set; }
	public TransactionStatus? StatusId { get; set; } = TransactionStatus.Pending;
	public UserReadDto? User { get; set; }
	public string? UserId { get; set; }
	public OrderReadDto? Order { get; set; }
	public Guid? OrderId { get; set; }
}

public class UserReadDto : IdentityUser {
	public bool Suspend { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? Headline { get; set; }
	public string? Bio { get; set; }
	public string? AppUserName { get; set; }
	public string? AppPhoneNumber { get; set; }
	public string? AppEmail { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public string? Region { get; set; }
	public string? Activity { get; set; }
	public string? Color { get; set; }
	public string? State { get; set; }
	public string? StateTr1 { get; set; }
	public string? StateTr2 { get; set; }
	public string? Gender { get; set; }
	public string? GenderTr1 { get; set; }
	public string? GenderTr2 { get; set; }
	public string VisitedProducts { get; set; } = "";
	public string BookmarkedProducts { get; set; } = "";
	public string FollowingUsers { get; set; } = "";
	public string FollowedUsers { get; set; } = "";
	public string BlockedUsers { get; set; } = "";
	public double? Wallet { get; set; } = 0;
	public double? Point { get; set; } = 0;
	public bool? ShowContactInfo { get; set; }
	public bool IsLoggedIn { get; set; }
	public DateTime? Birthdate { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public DateTime? DeletedAt { get; set; }
	public string? AccessLevel { get; set; }
	public string? Badge { get; set; }
	public bool IsOnline { get; set; } = false;
	public IEnumerable<MediaReadDto>? Media { get; set; }
	public IEnumerable<CategoryReadDto>? Categories { get; set; }
	public bool IsFollowing { get; set; } = false;
	public int? CountProducts { get; set; }
	public int? CountFollowers { get; set; } = 0;
	public int? CountFollowing { get; set; } = 0;
	public bool IsAdmin { get; set; }
	public string? Token { get; set; }
}