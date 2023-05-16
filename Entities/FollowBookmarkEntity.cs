namespace Utilities_aspnet.Entities;

[Table("Bookmarks")]
public class BookmarkEntity : BaseEntity {
	[SwaggerIgnore]
	public UserEntity? User { get; set; }

	[SwaggerIgnore]
	public string? UserId { get; set; }

	[StringLength(500)]
	public string? FolderName { get; set; }

	public ProductEntity? Product { get; set; }

	[SwaggerIgnore]
	public Guid? ProductId { get; set; }

	public CategoryEntity? Category { get; set; }

	[SwaggerIgnore]
	public Guid? CategoryId { get; set; }

	public Guid? ParentId { get; set; }
	public BookmarkEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<BookmarkEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class BookmarkCreateDto {
	public string? FolderName { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? ParentId { get; set; }
}

public class FollowCreateDto {
	public string UserId { get; set; } = null!;
}