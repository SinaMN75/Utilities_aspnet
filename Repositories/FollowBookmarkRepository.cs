namespace Utilities_aspnet.Repositories;

public interface IFollowBookmarkRepository {
	GenericResponse<IQueryable<UserEntity>> GetFollowers(string id);
	GenericResponse<IQueryable<UserEntity>> GetFollowing(string id);
	Task<GenericResponse<FollowEntity?>> ToggleFollow(FollowCreateDto dto);
	Task<GenericResponse> RemoveFollowings(string targetUserId, FollowCreateDto dto);
	GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId);
	Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto);
	Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto);
	GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarksByFolderName(string? folderName, string userId);
}

public class FollowBookmarkRepository : IFollowBookmarkRepository {
	private readonly DbContext _dbContext;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IUserRepository _userRepository;
	private readonly INotificationRepository _notificationRepository;

	public FollowBookmarkRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor,
		INotificationRepository notificationRepository,
		IUserRepository userRepository) {
		_dbContext = dbContext;
		_httpContextAccessor = httpContextAccessor;
		_notificationRepository = notificationRepository;
		_userRepository = userRepository;
	}

	public async Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto) {
		string userId = _httpContextAccessor.HttpContext!.User.Identity!.Name!;

		BookmarkEntity? oldBookmark = _dbContext.Set<BookmarkEntity>()
			.FirstOrDefault(x => (x.ProductId != null && x.ProductId == dto.ProductId ||
			                      x.CategoryId != null && x.CategoryId == dto.CategoryId) &&
			                     x.UserId == userId);
		if (oldBookmark == null) {
			BookmarkEntity bookmark = new() {UserId = userId};
			if (dto.ProductId.HasValue) bookmark.ProductId = dto.ProductId;
			bookmark.FolderName = dto.FolderName;
			await _dbContext.Set<BookmarkEntity>().AddAsync(bookmark);
			await _dbContext.SaveChangesAsync();
			return new GenericResponse<BookmarkEntity?>(bookmark, UtilitiesStatusCodes.Success, "Mission Accomplished");
		}
		_dbContext.Set<MediaEntity>().Remove((await _dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.BookmarkId == oldBookmark.Id))!);
		_dbContext.Set<BookmarkEntity>().Remove(oldBookmark);
		await _dbContext.SaveChangesAsync();

		GenericResponse<UserEntity?> userRespository = await _userRepository.ReadById(userId);
		UserEntity user = userRespository.Result!;
		if (user.BookmarkedProducts.Contains(dto.ProductId.ToString())) {
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = userId,
				BookmarkedProducts = user.BookmarkedProducts.Replace($",{dto.ProductId}", "")
			});
		}
		else {
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = userId,
				BookmarkedProducts = user.BookmarkedProducts + "," + dto.ProductId
			});
		}
		return new GenericResponse<BookmarkEntity?>(oldBookmark, UtilitiesStatusCodes.Success, "Mission Accomplished");
	}

	public GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId) {
		string uId = userId ?? _httpContextAccessor.HttpContext!.User.Identity!.Name!;
		IQueryable<BookmarkEntity> bookmark = _dbContext.Set<BookmarkEntity>().Include(x => x.Media)
			.Where(x => x.UserId == uId)
			.Include(x => x.Product).ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Votes)
			.Include(x => x.Product).ThenInclude(i => i.User).ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Bookmarks)
			.Include(x => x.Product).ThenInclude(i => i.Forms)!.ThenInclude(x => x.FormField)
			.Include(x => x.Product).ThenInclude(i => i.Categories)
			.Include(x => x.Product).ThenInclude(i => i.Comments.Where(x => x.ParentId == null)).ThenInclude(x => x.Children)
			.Include(x => x.Product).ThenInclude(i => i.Reports)
			.Include(x => x.Product).ThenInclude(i => i.Teams)!.ThenInclude(x => x.User)!.ThenInclude(x => x.Media);
		return new GenericResponse<IQueryable<BookmarkEntity>?>(bookmark);
	}

	public GenericResponse<IQueryable<UserEntity>> GetFollowers(string id) {
		IQueryable<UserEntity?> followers = _dbContext.Set<FollowEntity>()
			.Where(x => x.FollowsUserId == id)
			.Include(x => x.FollowerUser).ThenInclude(x => x.Media)
			.Include(x => x.FollowerUser).ThenInclude(x => x.Categories).ThenInclude(x => x.Media)
			.AsNoTracking().Select(x => x.FollowerUser);

		return new GenericResponse<IQueryable<UserEntity>>(followers);
	}

	public GenericResponse<IQueryable<UserEntity>> GetFollowing(string id) {
		IQueryable<UserEntity?> followings = _dbContext.Set<FollowEntity>()
			.Where(x => x.FollowerUserId == id)
			.Include(x => x.FollowsUser).ThenInclude(x => x.Media)
			.Include(x => x.FollowsUser).ThenInclude(x => x.Categories).ThenInclude(x => x.Media)
			.AsNoTracking()
			.Select(x => x.FollowsUser);

		return new GenericResponse<IQueryable<UserEntity>>(followings);
	}

	public async Task<GenericResponse<FollowEntity?>> ToggleFollow(FollowCreateDto parameters) {
		string uId = _httpContextAccessor.HttpContext!.User.Identity!.Name!;
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == uId))!;

		FollowEntity? follow = await _dbContext.Set<FollowEntity>()
			.FirstOrDefaultAsync(x => x.FollowerUserId == uId && x.FollowsUserId == parameters.UserId);

		if (follow != null) {
			_dbContext.Set<FollowEntity>().Remove(follow);
			await _dbContext.SaveChangesAsync();
		}
		else {
			follow = new FollowEntity {FollowerUserId = uId, FollowsUserId = parameters.UserId};
			await _dbContext.Set<FollowEntity>().AddAsync(follow);
			await _dbContext.SaveChangesAsync();

			UserEntity? followsUser = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == parameters.UserId);

			if (followsUser != null) {
				followsUser.Point += 1;
				await _dbContext.SaveChangesAsync();
			}

			try {
				GenericResponse<IQueryable<NotificationEntity>> notification = _notificationRepository.Filter(new NotificationFilterDto {
					UserId = parameters.UserId,
					CreatorUserId = uId,
					UseCase = "Follow"
				});
				if (notification.Result.IsNullOrEmpty())
					await _notificationRepository.Create(new NotificationCreateUpdateDto {
						UserId = parameters.UserId,
						Message = "You are being followed by " + myUser.UserName,
						Title = "Follow",
						UseCase = "Follow",
						CreatorUserId = uId
					});
			}
			catch { }

			if (myUser.FollowedUsers.Contains(parameters.UserId)) {
				await _userRepository.Update(new UserCreateUpdateDto {
					Id = uId,
					FollowedUsers = myUser.FollowedUsers.Replace($",{parameters.UserId}", "")
				});
			}
			else {
				await _userRepository.Update(new UserCreateUpdateDto {
					Id = uId,
					FollowedUsers = myUser.FollowedUsers + "," + parameters.UserId
				});
			}
		}

		return new GenericResponse<FollowEntity>(follow, UtilitiesStatusCodes.Success, "Mission Accomplished");
	}

	public async Task<GenericResponse> RemoveFollowings(string userId, FollowCreateDto parameters) {
		FollowEntity? following = await _dbContext.Set<FollowEntity>()
			.Where(x => x.FollowerUserId == parameters.UserId && x.FollowsUserId == userId).FirstOrDefaultAsync();
		if (following != null) {
			_dbContext.Set<FollowEntity>().Remove(following);
			await _dbContext.SaveChangesAsync();
		}

		return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
	}

	public GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarksByFolderName(string? folderName, string userId) {
		IQueryable<BookmarkEntity> bookmark = _dbContext.Set<BookmarkEntity>().Include(x => x.Media)
			.Where(x => x.UserId == userId)
			.Include(x => x.Product).ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Votes)
			.Include(x => x.Product).ThenInclude(i => i.User).ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Bookmarks)
			.Include(x => x.Product).ThenInclude(i => i.Forms)!.ThenInclude(x => x.FormField)
			.Include(x => x.Product).ThenInclude(i => i.Categories)
			.Include(x => x.Product).ThenInclude(i => i.Comments.Where(x => x.ParentId == null)).ThenInclude(x => x.Children)
			.Include(x => x.Product).ThenInclude(i => i.Reports)
			.Include(x => x.Product).ThenInclude(i => i.Teams)!.ThenInclude(x => x.User)!.ThenInclude(x => x.Media);

		if (folderName.IsNotNullOrEmpty()) bookmark = bookmark.Where(x => x.FolderName == folderName);
		return new GenericResponse<IQueryable<BookmarkEntity>?>(bookmark);
	}

	public async Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) {
		string userId = _httpContextAccessor.HttpContext!.User.Identity!.Name!;

		BookmarkEntity? oldBookmark = await _dbContext.Set<BookmarkEntity>()
			.FirstOrDefaultAsync(x => x.Id == bookmarkId);
		if (oldBookmark is null) {
			return new GenericResponse<BookmarkEntity?>(null, UtilitiesStatusCodes.NotFound, "Not Found");
		}

		oldBookmark.FolderName = dto.FolderName ?? oldBookmark.FolderName;
		_dbContext.Set<BookmarkEntity>().Update(oldBookmark);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<BookmarkEntity?>(oldBookmark, UtilitiesStatusCodes.Success, "Mission Accomplished");
	}
}