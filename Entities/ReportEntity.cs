namespace Utilities_aspnet.Entities;

[Table("Reports")]
public class ReportEntity : BaseEntity {
	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? Description { get; set; }

	public string? CreatorUserId { get; set; }
	public UserEntity? CreatorUser { get; set; }

	public string? UserId { get; set; }
	public UserEntity? User { get; set; }

	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }

	public CommentEntity? Comment { get; set; }
	public Guid? CommentId { get; set; }

	public ChatEntity? Chat { get; set; }
	public Guid? ChatId { get; set; }

	public GroupChatMessageEntity? GroupChatMessage { get; set; }
	public Guid? GroupChatMessageId { get; set; }

	public GroupChatEntity? GroupChat { get; set; }
	public Guid? GroupChatId { get; set; }

	public ReportType ReportType { get; set; } = ReportType.All;
}

public class ReportFilterDto {
	public bool? User { get; set; }
	public bool? Product { get; set; }
	public bool? Chat { get; set; }
	public bool? GroupChatMessage { get; set; }
	public bool? GroupChat { get; set; }
	public ReportType ReportType { get; set; }
	public string? UserId { get; set; }
	public int? Count { get; set; }
}