namespace Utilities_aspnet.Entities;

[Table("Bookmarks")]
public class BookmarkEntity : BaseEntity {
	public string? FolderName { get; set; }

	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public BookmarkEntity? Parent { get; set; }
	public Guid? ParentId { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<BookmarkEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class BookmarkCreateDto {
	public string? FolderName { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? ParentId { get; set; }
}

public class FollowCreateDto {
	public string UserId { get; set; } = null!;
}