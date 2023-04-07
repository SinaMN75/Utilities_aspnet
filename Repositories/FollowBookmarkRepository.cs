﻿namespace Utilities_aspnet.Repositories;

public interface IFollowBookmarkRepository {
	Task<GenericResponse<IQueryable<UserEntity>>> GetFollowers(string id);
	Task<GenericResponse<IQueryable<UserEntity>>> GetFollowing(string id);
	Task<GenericResponse> ToggleFollow(FollowCreateDto dto);
	Task<GenericResponse> RemoveFollowings(string targetUserId, FollowCreateDto dto);
	GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId);
	Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto);
	Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto);
}

public class FollowBookmarkRepository : IFollowBookmarkRepository {
	private readonly DbContext _dbContext;
	private readonly IUserRepository _userRepository;
	private readonly INotificationRepository _notificationRepository;
	private readonly string _userId;

	public FollowBookmarkRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor,
		INotificationRepository notificationRepository,
		IUserRepository userRepository) {
		_dbContext = dbContext;
		_notificationRepository = notificationRepository;
		_userRepository = userRepository;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name!;
	}

	public async Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto) {
		BookmarkEntity? oldBookmark = _dbContext.Set<BookmarkEntity>()
			.FirstOrDefault(x => (x.ProductId != null && x.ProductId == dto.ProductId ||
			                      x.CategoryId != null && x.CategoryId == dto.CategoryId) &&
			                     x.UserId == _userId);
		if (oldBookmark == null) {
			BookmarkEntity bookmark = new() {UserId = _userId};
			if (dto.ProductId.HasValue) bookmark.ProductId = dto.ProductId;
			bookmark.FolderName = dto.FolderName;
			await _dbContext.Set<BookmarkEntity>().AddAsync(bookmark);
			await _dbContext.SaveChangesAsync();
			return new GenericResponse<BookmarkEntity?>(bookmark, UtilitiesStatusCodes.Success);
		}
		MediaEntity? media = await _dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.BookmarkId == oldBookmark.Id);
		if (media != null) _dbContext.Set<MediaEntity>().Remove(media);
		_dbContext.Set<BookmarkEntity>().Remove(oldBookmark);
		await _dbContext.SaveChangesAsync();

		GenericResponse<UserEntity?> userRespository = await _userRepository.ReadById(_userId);
		UserEntity user = userRespository.Result!;
		if (user.BookmarkedProducts.Contains(dto.ProductId.ToString())) {
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				BookmarkedProducts = user.BookmarkedProducts.Replace($",{dto.ProductId}", "")
			});
		}
		else {
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				BookmarkedProducts = user.BookmarkedProducts + "," + dto.ProductId
			});
		}
		return new GenericResponse<BookmarkEntity?>(oldBookmark, UtilitiesStatusCodes.Success);
	}

	public GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId) {
		string uId = userId ?? _userId;
		IQueryable<BookmarkEntity> bookmark = _dbContext.Set<BookmarkEntity>().Include(x => x.Media)
			.Where(x => x.UserId == uId)
			.Include(x => x.Product).ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Votes)
			.Include(x => x.Product).ThenInclude(i => i.User).ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Bookmarks)
			.Include(x => x.Product).ThenInclude(i => i.Forms)!.ThenInclude(x => x.FormField)
			.Include(x => x.Product).ThenInclude(i => i.Categories)
			.Include(x => x.Product).ThenInclude(i => i.Comments.Where(x => x.ParentId == null)).ThenInclude(x => x.Children)
			.Include(x => x.Product).ThenInclude(i => i.Reports);
		return new GenericResponse<IQueryable<BookmarkEntity>?>(bookmark);
	}

	public async Task<GenericResponse<IQueryable<UserEntity>>> GetFollowers(string id) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId))!;
		GenericResponse<IQueryable<UserEntity>> q = await _userRepository.Filter(new UserFilterDto {
				UserIds = myUser.FollowedUsers.Split(","),
				ShowCategories = true,
				ShowMedia = true
			}
		);
		return new GenericResponse<IQueryable<UserEntity>>(q.Result!);
	}

	public async Task<GenericResponse<IQueryable<UserEntity>>> GetFollowing(string id) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId))!;
		GenericResponse<IQueryable<UserEntity>> q = await _userRepository.Filter(new UserFilterDto {
				UserIds = myUser.FollowingUsers.Split(","),
				ShowCategories = true,
				ShowMedia = true
			}
		);
		return new GenericResponse<IQueryable<UserEntity>>(q.Result!);
	}

	public async Task<GenericResponse> ToggleFollow(FollowCreateDto parameters) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId))!;
		UserEntity targetUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == parameters.UserId))!;

		// FollowEntity? follow = await _dbContext.Set<FollowEntity>()
		// .FirstOrDefaultAsync(x => x.FollowerUserId == _userId && x.FollowsUserId == parameters.UserId);

		// if (follow != null) {
		// _dbContext.Set<FollowEntity>().Remove(follow);
		// await _dbContext.SaveChangesAsync();
		// }
		// else {
		// follow = new FollowEntity {FollowerUserId = _userId, FollowsUserId = parameters.UserId};
		// await _dbContext.Set<FollowEntity>().AddAsync(follow);
		// await _dbContext.SaveChangesAsync();

		// if (targetUser != null) {
		// 	targetUser.Point += 1;
		// 	await _dbContext.SaveChangesAsync();
		// }

		try {
			GenericResponse<IQueryable<NotificationEntity>> notification = _notificationRepository.Filter(new NotificationFilterDto {
				UserId = parameters.UserId,
				CreatorUserId = _userId,
				UseCase = "Follow"
			});
			if (notification.Result.IsNullOrEmpty())
				await _notificationRepository.Create(new NotificationCreateUpdateDto {
					UserId = parameters.UserId,
					Message = "You are being followed by " + myUser.UserName,
					Title = "Follow",
					UseCase = "Follow",
					CreatorUserId = _userId
				});
		}
		catch { }

		if (myUser.FollowingUsers.Contains(parameters.UserId)) {
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				FollowingUsers = myUser.FollowingUsers.Replace($",{parameters.UserId}", "")
			});

			await _userRepository.Update(new UserCreateUpdateDto {
				Id = parameters.UserId,
				FollowedUsers = targetUser.FollowedUsers.Replace($",{parameters.UserId}", "")
			});
		}
		else {
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				FollowingUsers = myUser.FollowingUsers + "," + parameters.UserId
			});

			await _userRepository.Update(new UserCreateUpdateDto {
				Id = parameters.UserId,
				FollowingUsers = targetUser.FollowedUsers + "," + parameters.UserId
			});
		}
		return new GenericResponse();
	}

	public async Task<GenericResponse> RemoveFollowings(string userId, FollowCreateDto parameters) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId))!;
		await _userRepository.Update(new UserCreateUpdateDto {
			Id = parameters.UserId,
			FollowedUsers = myUser.FollowedUsers.Replace($",{parameters.UserId}", "")
		});
		return new GenericResponse();
	}

	public async Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) {
		BookmarkEntity? oldBookmark = await _dbContext.Set<BookmarkEntity>()
			.FirstOrDefaultAsync(x => x.Id == bookmarkId);
		if (oldBookmark is null) {
			return new GenericResponse<BookmarkEntity?>(null, UtilitiesStatusCodes.NotFound);
		}

		oldBookmark.FolderName = dto.FolderName ?? oldBookmark.FolderName;
		_dbContext.Set<BookmarkEntity>().Update(oldBookmark);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<BookmarkEntity?>(oldBookmark);
	}
}