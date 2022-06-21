﻿namespace Utilities_aspnet.Entities;

[Table("Follows")]
public class FollowEntity : BaseEntity {
	public string? FollowerUserId { get; set; }
	public UserEntity? FollowerUser { get; set; }

	public string? FollowsUserId { get; set; }
	public UserEntity? FollowsUser { get; set; }
}

[Table("Bookmarks")]
public class BookmarkEntity : BaseEntity {
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	public CategoryEntity? Category { get; set; }
	public Guid? CategoryId { get; set; }
}

public class BookmarkCreateDto {
	public Guid? ProductId { get; set; }
	public Guid? CategoryId { get; set; }
}

public class BookmarkReadDto {
	public IEnumerable<UserReadDto>? Users { get; set; }
	public IEnumerable<ProductReadDto>? Products { get; set; }
	public IEnumerable<ProductReadDto>? Categories { get; set; }
}

public class FollowReadDto {
	public IEnumerable<UserReadDto>? Followers { get; set; }
}

public class FollowCreateDto {
	public string UserId { get; set; } = null!;
}

public class FollowingReadDto {
	public IEnumerable<UserReadDto>? Followings { get; set; }
}