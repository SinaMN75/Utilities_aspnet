﻿namespace Utilities_aspnet.Entities;

// [Table("Follows")]
// public class FollowEntity : BaseEntity {
// 	/// <summary>
// 	/// دنبال شده
// 	/// </summary>
// 	public string? FollowerUserId { get; set; }
//
// 	public UserEntity? FollowerUser { get; set; }
//
// 	/// <summary>
// 	/// دنبال کننده
// 	/// </summary>
// 	public string? FollowsUserId { get; set; }
//
// 	public UserEntity? FollowsUser { get; set; }
// }

[Table("Bookmarks")]
public class BookmarkEntity : BaseEntity {
	[JsonIgnore]
	public UserEntity? User { get; set; }

	[JsonIgnore]
	public string? UserId { get; set; }

	[StringLength(500)]
	public string? FolderName { get; set; }

	public ProductEntity? Product { get; set; }

	[JsonIgnore]
	public Guid? ProductId { get; set; }

	public CategoryEntity? Category { get; set; }

	[JsonIgnore]
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