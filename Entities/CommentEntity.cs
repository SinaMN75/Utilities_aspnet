namespace Utilities_aspnet.Entities;

[Table("Comment")]
public class CommentEntity : BaseEntity {
	public double? Score { get; set; } = 0;

	[MaxLength(1000)]
	public CommentJsonDetail JsonDetail { get; set; } = new();

	[StringLength(2000)]
	public string? Comment { get; set; }

	public ChatStatus? Status { get; set; }

	public Guid? ParentId { get; set; }
	public CommentEntity? Parent { get; set; }

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CommentEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public ProductEntity? Product { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? ProductId { get; set; }
}

public class CommentJsonDetail {
	public List<CommentReacts> Reacts { get; set; } = new();
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
	public ChatStatus? Status { get; set; }
}

public class CommentFilterDto {
	public string? UserId { get; set; }
	public string? ProductOwnerId { get; set; }
	public Guid? ProductId { get; set; }
	public ChatStatus? Status { get; set; }
}