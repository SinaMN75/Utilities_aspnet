namespace Utilities_aspnet.Repositories;

public interface IFollowBookmarkRepository {
	GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId);
	Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto);
	Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto);
}

public class FollowBookmarkRepository(
	DbContext dbContext,
	IHttpContextAccessor httpContextAccessor,
	INotificationRepository notificationRepository,
	IUserRepository userRepository)
	: IFollowBookmarkRepository {
	private readonly string _userId = httpContextAccessor.HttpContext!.User.Identity!.Name!;

	public async Task<GenericResponse<BookmarkEntity?>> ToggleBookmark(BookmarkCreateDto dto) {
		BookmarkEntity? oldBookmark = dbContext.Set<BookmarkEntity>()
			.FirstOrDefault(x => x.ProductId != null && x.ProductId == dto.ProductId && x.UserId == _userId);
		if (oldBookmark == null) {
			BookmarkEntity e = new() { UserId = _userId };
			if (dto.ProductId.HasValue) e.ProductId = dto.ProductId;
			e.FolderName = dto.FolderName;
			e.ParentId = dto.ParentId;
			await dbContext.Set<BookmarkEntity>().AddAsync(e);
			await dbContext.SaveChangesAsync();
			return new GenericResponse<BookmarkEntity?>(e);
		}

		MediaEntity? media = await dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.BookmarkId == oldBookmark.Id);
		if (media != null) dbContext.Set<MediaEntity>().Remove(media);
		dbContext.Set<BookmarkEntity>().Remove(oldBookmark);
		await dbContext.SaveChangesAsync();

		GenericResponse<UserEntity?> userRespository = await userRepository.ReadById(_userId);
		UserEntity user = userRespository.Result!;
		if (user.BookmarkedProducts.Contains(dto.ProductId.ToString()!))
			await userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				BookmarkedProducts = user.BookmarkedProducts.Replace($",{dto.ProductId}", "")
			});
		else
			await userRepository.Update(new UserCreateUpdateDto {
				Id = _userId,
				BookmarkedProducts = user.BookmarkedProducts + "," + dto.ProductId
			});
		return new GenericResponse<BookmarkEntity?>(oldBookmark);
	}

	public GenericResponse<IQueryable<BookmarkEntity>?> ReadBookmarks(string? userId) {
		string uId = userId ?? _userId;
		IQueryable<BookmarkEntity> bookmark = dbContext.Set<BookmarkEntity>().AsNoTracking()
			.Include(x => x.Media)
			.Where(x => x.UserId == uId)
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Product).ThenInclude(x => x!.User).ThenInclude(x => x!.Media)
			.Include(x => x.Product).ThenInclude(x => x!.Parent)
			.Include(x => x.Product).ThenInclude(x => x!.Parent).ThenInclude(x => x!.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Product).ThenInclude(x => x!.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children).Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Children).Include(x => x.Product).ThenInclude(x => x!.Parent)
			.Include(x => x.Parent);
		return new GenericResponse<IQueryable<BookmarkEntity>?>(bookmark);
	}

	public async Task<GenericResponse<IQueryable<UserEntity>>> GetFollowers(string id) {
		UserEntity myUser = (await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!;
		GenericResponse<IQueryable<UserEntity>> q = await userRepository.Filter(new UserFilterDto {
				UserIds = myUser.FollowedUsers.Split(","),
				ShowCategories = true,
				ShowMedia = true
			}
		);
		return new GenericResponse<IQueryable<UserEntity>>(q.Result!);
	}

	public async Task<GenericResponse<BookmarkEntity?>> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) {
		BookmarkEntity? oldBookmark = await dbContext.Set<BookmarkEntity>().FirstOrDefaultAsync(x => x.Id == bookmarkId);
		if (oldBookmark is null) return new GenericResponse<BookmarkEntity?>(null, UtilitiesStatusCodes.NotFound);

		oldBookmark.FolderName = dto.FolderName ?? oldBookmark.FolderName;
		oldBookmark.ParentId = dto.ParentId;
		dbContext.Set<BookmarkEntity>().Update(oldBookmark);
		await dbContext.SaveChangesAsync();

		return new GenericResponse<BookmarkEntity?>(oldBookmark);
	}
}