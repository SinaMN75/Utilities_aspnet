﻿namespace Utilities_aspnet.Repositories;

public interface IFollowBookmarkRepository
{
    Task<GenericResponse<FollowReadDto>> GetFollowers(string id);
    Task<GenericResponse<FollowingReadDto>> GetFollowing(string id);
    Task<GenericResponse> ToggleFollow(string sourceUserId, FollowCreateDto dto);
    Task<GenericResponse> RemoveFollowings(string targetUserId, FollowCreateDto dto);
    Task<GenericResponse<IEnumerable<BookmarkReadDto>?>> ReadBookmarks();

    Task<GenericResponse> ToggleBookmark(BookmarkCreateDto dto);
    //Task<GenericResponse<IEnumerable<BookmarkFolderReadDto>?>> ReadBookmarkFolders();
    //Task<GenericResponse> CreateUpdateBookmarkFolder(BookmarkFolderCreateUpdateDto dto);
    //Task<GenericResponse> DeleteBookmarkFolder(Guid id);

}

public class FollowBookmarkRepository : IFollowBookmarkRepository
{
    private readonly DbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly INotificationRepository _notificationRepository;

    public FollowBookmarkRepository(
        DbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        INotificationRepository notificationRepository)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _notificationRepository = notificationRepository;
    }

    public async Task<GenericResponse> ToggleBookmark(BookmarkCreateDto dto)
    {
        BookmarkEntity? oldBookmark = _context.Set<BookmarkEntity>()
            .FirstOrDefault(x => (
                                     ((x.ProductId != null && x.ProductId == dto.ProductId) ||
                                     (x.CategoryId != null && x.CategoryId == dto.CategoryId)) &&
                                 x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!));
        if (oldBookmark == null)
        {
            BookmarkEntity bookmark = new() { UserId = _httpContextAccessor.HttpContext!.User.Identity!.Name! };

            if (dto.ProductId.HasValue) bookmark.ProductId = dto.ProductId;
            bookmark.FolderName = dto.FolderName;

            await _context.Set<BookmarkEntity>().AddAsync(bookmark);
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.Set<BookmarkEntity>().Remove(oldBookmark);
            await _context.SaveChangesAsync();
        }

        return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
    }

    public async Task<GenericResponse<IEnumerable<BookmarkReadDto>?>> ReadBookmarks()
    {
        IEnumerable<BookmarkEntity>? bookmark = await _context.Set<BookmarkEntity>().Where(x => x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!)
            .Include(x => x.Product).ThenInclude(x => x.Media).Include(x => x.Product).ThenInclude(i => i.Votes).Include(x => x.Product).ThenInclude(i => i.User)!.ThenInclude(x => x.Media)
            .Include(x => x.Product).ThenInclude(i => i.Bookmarks).Include(x => x.Product).ThenInclude(i => i.Forms)!.ThenInclude(x => x.FormField)
            .Include(x => x.Product).ThenInclude(i => i.Categories).Include(x => x.Product).ThenInclude(i => i.Comments.Where(x => x.ParentId == null))!.ThenInclude(x => x.Children)
            .Include(x => x.Product).ThenInclude(i => i.Locations).Include(x => x.Product).ThenInclude(i => i.Reports).Include(x => x.Product).ThenInclude(i => i.Teams)!
            .ThenInclude(x => x.User)!.ThenInclude(x => x.Media).ToListAsync();


        return new GenericResponse<IEnumerable<BookmarkReadDto>?>(_mapper.Map<IEnumerable<BookmarkReadDto>>(bookmark));
    }

    //public async Task<GenericResponse<BookmarkReadDto>> ReadBookmarksOld() {
    //	GenericResponse<IEnumerable<ProductReadDto>> products =
    //		await _productRepository.Read(new FilterProductDto {IsBookmarked = true});

    //	BookmarkReadDto dto = new() {
    //		Products = products.Result
    //	};

    //	return new GenericResponse<BookmarkReadDto?>(_mapper.Map<BookmarkReadDto>(dto));
    //}

    //public async Task<GenericResponse> CreateUpdateBookmarkFolder(BookmarkFolderCreateUpdateDto dto) {
    //	BookmarkFolderEntity? oldBookmarkFolder = _context.Set<BookmarkFolderEntity>()
    //		.FirstOrDefault(x => ( x.Id == dto.Id && x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!));
    //	if (oldBookmarkFolder == null) {
    //		BookmarkFolderEntity bookmarkFolder = new() {UserId = _httpContextAccessor.HttpContext!.User.Identity!.Name! , Title = dto.Title};

    //		await _context.Set<BookmarkFolderEntity>().AddAsync(bookmarkFolder);
    //		await _context.SaveChangesAsync();
    //	}
    //	else {
    //		oldBookmarkFolder.Title = dto.Title;
    //		await _context.SaveChangesAsync();
    //	}

    //	return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
    //}

    //public async Task<GenericResponse<IEnumerable<BookmarkFolderReadDto>?>> ReadBookmarkFolders() {

    //	IEnumerable<BookmarkFolderEntity>? bookmarkFolders = await _context.Set<BookmarkFolderEntity>().Where(x => x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!).ToListAsync();


    //	return new GenericResponse<IEnumerable<BookmarkFolderReadDto>?>(_mapper.Map<IEnumerable<BookmarkFolderReadDto>?>(bookmarkFolders));
    //}

    //public async Task<GenericResponse> DeleteBookmarkFolder(Guid id)
    //{
    //	BookmarkFolderEntity? bookmarkFolder = _context.Set<BookmarkFolderEntity>().Include(x=>x.Bookmarks)
    //		.FirstOrDefault(x => (x.Id == id && x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!));
    //	if (bookmarkFolder != null)
    //	{

    //		_context.Set<BookmarkFolderEntity>().Remove(bookmarkFolder);
    //		await _context.SaveChangesAsync();
    //	}

    //	return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
    //}

    public async Task<GenericResponse<FollowReadDto>> GetFollowers(string id)
    {
        IEnumerable<UserEntity?> followers = await _context.Set<FollowEntity>()
            .AsNoTracking()
            .Where(x => x.FollowsUserId == id)
            .Include(x => x.FollowerUser)
            .ThenInclude(x => x.Media)
            .Select(x => x.FollowerUser)
            .ToListAsync();

        IEnumerable<UserReadDto>? users = _mapper.Map<IEnumerable<UserReadDto>>(followers);

        return new GenericResponse<FollowReadDto>(new FollowReadDto { Followers = users });
    }

    public async Task<GenericResponse<FollowingReadDto>> GetFollowing(string id)
    {
        IEnumerable<UserEntity?> followings = await _context.Set<FollowEntity>()
            .AsNoTracking()
            .Where(x => x.FollowerUserId == id)
            .Include(x => x.FollowsUser)
            .ThenInclude(x => x.Media)
            .Select(x => x.FollowsUser)
            .ToListAsync();

        IEnumerable<UserReadDto>? users = _mapper.Map<IEnumerable<UserReadDto>>(followings);

        return new GenericResponse<FollowingReadDto>(new FollowingReadDto { Followings = users });
    }

    public async Task<GenericResponse> ToggleFollow(string sourceUserId, FollowCreateDto parameters)
    {

        UserEntity? myUser = await _context.Set<UserEntity>().FirstOrDefaultAsync(x=>x.Id == sourceUserId);
        FollowEntity? follow = await _context.Set<FollowEntity>()
            .FirstOrDefaultAsync(x => x.FollowerUserId == sourceUserId && x.FollowsUserId == parameters.UserId);
        if (follow != null)
        {
            _context.Set<FollowEntity>().Remove(follow);
        }
        else
        {
            follow = new FollowEntity
            {
                FollowerUserId = sourceUserId,
                FollowsUserId = parameters.UserId
            };

            await _context.Set<FollowEntity>().AddAsync(follow);
            try
            {
                _notificationRepository.CreateNotification(new NotificationCreateUpdateDto { UserId = parameters.UserId, Message = "You are being followed by " + myUser.UserName, Title = "Follow" });
            }
            catch { }

        }

        await _context.SaveChangesAsync();

        return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
    }

    public async Task<GenericResponse> RemoveFollowings(string userId, FollowCreateDto parameters)
    {

        FollowEntity? following = await _context.Set<FollowEntity>()
            .Where(x => x.FollowerUserId == parameters.UserId && x.FollowsUserId == userId)
            .FirstOrDefaultAsync();
        if (following != null)
        {
            _context.Set<FollowEntity>().Remove(following);
            await _context.SaveChangesAsync();
        }


        return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
    }
}