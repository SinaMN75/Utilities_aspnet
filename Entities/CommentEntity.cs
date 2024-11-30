namespace Utilities_aspnet.Entities;

[Table("Comment")]
public class CommentEntity : BaseEntity {
	public double? Score { get; set; } = 0;

	public CommentJsonDetail JsonDetail { get; set; } = new();

	[MaxLength(2000)]
	public string? Comment { get; set; }

	public Guid? ParentId { get; set; }
	public CommentEntity? Parent { get; set; }

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
	public UserEntity? TargetUser { get; set; }
	public string? TargetUserId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	[MaxLength(100)]
	public List<TagComment> Tags { get; set; } = [];

	[InverseProperty("Parent")]
	public IEnumerable<CommentEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public IEnumerable<ReportEntity>? Reports { get; set; }

	public IEnumerable<NotificationEntity>? Notifications { get; set; }
}

public class CommentJsonDetail {
	public List<CommentReacts> Reacts { get; set; } = [];
}

public class CommentReacts {
	public Reaction Reaction { get; set; }
	public string UserId { get; set; } = null!;
}

public class CommentCreateUpdateDto {
	public Guid? ParentId { get; set; }
	public double? Score { get; set; }
	public string? Comment { get; set; }
	public Guid? ProductId { get; set; }
	public string? UserId { get; set; }
	public List<TagComment>? Tags { get; set; }
}

public class CommentFilterDto : BaseFilterDto {
	public string? UserId { get; set; }
	public string? ProductOwnerId { get; set; }
	public Guid? ProductId { get; set; }
	public string? TargetUserId { get; set; }
	public List<TagComment>? Tags { get; set; }
}