namespace Utilities_aspnet.Entities;

[Table("Reports")]
public class ReportEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(500)]
	public string? Description { get; set; }

	public string? CreatorUserId { get; set; }
	public UserEntity? CreatorUser { get; set; }

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public CommentEntity? Comment { get; set; }
	public Guid? CommentId { get; set; }
}

public class ReportCreateUpdateDto {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? UserId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? CommentId { get; set; }
}