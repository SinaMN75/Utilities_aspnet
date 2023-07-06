namespace Utilities_aspnet.Repositories;

public interface IFollowBookmarkRepository {
	Task<GenericResponse<IQueryable<UserEntity>>> GetFollowers(string id);
	Task<GenericResponse<IQueryable<UserEntity>>> GetFollowing(string id);
	Task<GenericResponse> ToggleFollow(FollowCreateDto dto);
	Task<GenericResponse> RemoveFollowings(FollowCreateDto dto);
	GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId);
	Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto);
	Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto);
}

public class FollowBookmarkRepository : IFollowBookmarkRepository {
	private readonly DbContext _dbContext;
	private readonly INotificationRepository _notificationRepository;
	private readonly string _userId;
	private readonly IUserRepository _userRepository;

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
			.FirstOrDefault(x => x.ProductId != null && x.ProductId == dto.ProductId && x.UserId == _userId);
		if (oldBookmark == null) {
			BookmarkEntity e = new() { UserId = _userId };
			if (dto.ProductId.HasValue) e.ProductId = dto.ProductId;
			e.FolderName = dto.FolderName;
			e.ParentId = dto.ParentId;
			await _dbContext.Set<BookmarkEntity>().AddAsync(e);
			await _dbContext.SaveChangesAsync();
			return new GenericResponse<BookmarkEntity?>(e);
		}
		MediaEntity? media = await _dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.BookmarkId == oldBookmark.Id);
		if (media != null) _dbContext.Set<MediaEntity>().Remove(media);
		_dbContext.Set<BookmarkEntity>().Remove(oldBookmark);
		await _dbContext.SaveChangesAsync();

		GenericResponse<UserEntity?> userRespository = await _userRepository.ReadById(_userId);
		UserEntity user = userRespository.Result!;
		if (user.BookmarkedProducts.Contains(dto.ProductId.ToString()!))
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				BookmarkedProducts = user.BookmarkedProducts.Replace($",{dto.ProductId}", "")
			});
		else
			await _userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				BookmarkedProducts = user.BookmarkedProducts + "," + dto.ProductId
			});
		return new GenericResponse<BookmarkEntity?>(oldBookmark);
	}

	public GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId) {
		string uId = userId ?? _userId;
		IQueryable<BookmarkEntity> bookmark = _dbContext.Set<BookmarkEntity>().Include(x => x.Media)
			.Where(x => x.UserId == uId)
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Product)
			.Include(x => x.Children).Include(x => x.Product).ThenInclude(x => x!.Media);
		return new GenericResponse<IQueryable<BookmarkEntity>?>(bookmark);
	}

	public async Task<GenericResponse<IQueryable<UserEntity>>> GetFollowers(string id) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!;
		GenericResponse<IQueryable<UserEntity>> q = _userRepository.Filter(new UserFilterDto {
				UserIds = myUser.FollowedUsers.Split(","),
				ShowCategories = true,
				ShowMedia = true
			}
		);
		return new GenericResponse<IQueryable<UserEntity>>(q.Result!);
	}

	public async Task<GenericResponse<IQueryable<UserEntity>>> GetFollowing(string id) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!;
		GenericResponse<IQueryable<UserEntity>> q = _userRepository.Filter(new UserFilterDto {
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
				FollowedUsers = targetUser.FollowedUsers + "," + parameters.UserId
			});
		}
		return new GenericResponse();
	}

	public async Task<GenericResponse> RemoveFollowings(FollowCreateDto parameters) {
		UserEntity myUser = (await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == _userId))!;
		await _userRepository.Update(new UserCreateUpdateDto {
			Id = parameters.UserId,
			FollowedUsers = myUser.FollowedUsers.Replace($",{parameters.UserId}", "")
		});
		return new GenericResponse();
	}

	public async Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) {
		BookmarkEntity? oldBookmark = await _dbContext.Set<BookmarkEntity>().FirstOrDefaultAsync(x => x.Id == bookmarkId);
		if (oldBookmark is null) return new GenericResponse<BookmarkEntity?>(null, UtilitiesStatusCodes.NotFound);

		oldBookmark.FolderName = dto.FolderName ?? oldBookmark.FolderName;
		oldBookmark.ParentId = dto.ParentId;
		_dbContext.Set<BookmarkEntity>().Update(oldBookmark);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<BookmarkEntity?>(oldBookmark);
	}
}