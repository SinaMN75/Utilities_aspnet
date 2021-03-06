namespace Utilities_aspnet.Repositories;

public interface IFollowBookmarkRepository {
	Task<GenericResponse<IEnumerable<UserReadDto>>> GetFollowers(string id);
	Task<GenericResponse<IEnumerable<UserReadDto>>> GetFollowing(string id);
	Task<GenericResponse> ToggleFollow(string sourceUserId, FollowCreateDto dto);
	Task<GenericResponse> RemoveFollowings(string targetUserId, FollowCreateDto dto);
	Task<GenericResponse<IEnumerable<BookmarkReadDto>?>> ReadBookmarks();

	Task<GenericResponse> ToggleBookmark(BookmarkCreateDto dto);
}

public class FollowBookmarkRepository : IFollowBookmarkRepository {
	private readonly DbContext _context;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IMapper _mapper;
	private readonly INotificationRepository _notificationRepository;

	public FollowBookmarkRepository(
		DbContext context,
		IMapper mapper,
		IHttpContextAccessor httpContextAccessor,
		INotificationRepository notificationRepository) {
		_context = context;
		_mapper = mapper;
		_httpContextAccessor = httpContextAccessor;
		_notificationRepository = notificationRepository;
	}

	public async Task<GenericResponse> ToggleBookmark(BookmarkCreateDto dto) {
		BookmarkEntity? oldBookmark = _context.Set<BookmarkEntity>()
			.FirstOrDefault(x => (
				                ((x.ProductId != null && x.ProductId == dto.ProductId) ||
				                 (x.CategoryId != null && x.CategoryId == dto.CategoryId)) &&
				                x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!));
		if (oldBookmark == null) {
			BookmarkEntity bookmark = new() {UserId = _httpContextAccessor.HttpContext!.User.Identity!.Name!};

			if (dto.ProductId.HasValue) bookmark.ProductId = dto.ProductId;
			bookmark.FolderName = dto.FolderName;

			await _context.Set<BookmarkEntity>().AddAsync(bookmark);
			await _context.SaveChangesAsync();
		}
		else {
			_context.Set<BookmarkEntity>().Remove(oldBookmark);
			await _context.SaveChangesAsync();
		}

		return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
	}

	public async Task<GenericResponse<IEnumerable<BookmarkReadDto>?>> ReadBookmarks() {
		IEnumerable<BookmarkEntity> bookmark = await _context.Set<BookmarkEntity>()
			.Where(x => x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name!)
			.Include(x => x.Product).ThenInclude(x => x.Media).Include(x => x.Product).ThenInclude(i => i.Votes)
			.Include(x => x.Product).ThenInclude(i => i.User)!.ThenInclude(x => x.Media)
			.Include(x => x.Product).ThenInclude(i => i.Bookmarks).Include(x => x.Product).ThenInclude(i => i.Forms)!
			.ThenInclude(x => x.FormField)
			.Include(x => x.Product).ThenInclude(i => i.Categories).Include(x => x.Product)
			.ThenInclude(i => i.Comments.Where(x => x.ParentId == null))!.ThenInclude(x => x.Children)
			.Include(x => x.Product).ThenInclude(i => i.Locations).Include(x => x.Product).ThenInclude(i => i.Reports)
			.Include(x => x.Product).ThenInclude(i => i.Teams)!
			.ThenInclude(x => x.User)!.ThenInclude(x => x.Media).ToListAsync();

		return new GenericResponse<IEnumerable<BookmarkReadDto>?>(_mapper.Map<IEnumerable<BookmarkReadDto>>(bookmark));
	}

	public async Task<GenericResponse<IEnumerable<UserReadDto>>> GetFollowers(string id) {
		IEnumerable<UserEntity?> followers = await _context.Set<FollowEntity>()
			.AsNoTracking()
			.Where(x => x.FollowsUserId == id)
			.Include(x => x.FollowerUser)
			.ThenInclude(x => x.Media)
			.Include(x => x.FollowerUser)
			.ThenInclude(x => x.Categories).ThenInclude(x => x.Media)
			.Select(x => x.FollowerUser)
			.ToListAsync();

		IEnumerable<UserReadDto>? users = _mapper.Map<IEnumerable<UserReadDto>>(followers);

		return new GenericResponse<IEnumerable<UserReadDto>>(users);
	}

	public async Task<GenericResponse<IEnumerable<UserReadDto>>> GetFollowing(string id) {
		IEnumerable<UserEntity?> followings = await _context.Set<FollowEntity>()
			.AsNoTracking()
			.Where(x => x.FollowerUserId == id)
			.Include(x => x.FollowsUser)
			.ThenInclude(x => x.Media)
			.Include(x => x.FollowsUser)
			.ThenInclude(x => x.Categories).ThenInclude(x=>x.Media)
			.Select(x => x.FollowsUser)
			.ToListAsync();

		IEnumerable<UserReadDto>? users = _mapper.Map<IEnumerable<UserReadDto>>(followings);

		return new GenericResponse<IEnumerable<UserReadDto>>(users);
	}

	public async Task<GenericResponse> ToggleFollow(string sourceUserId, FollowCreateDto parameters) {
		UserEntity? myUser = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == sourceUserId);
		FollowEntity? follow = await _context.Set<FollowEntity>()
			.FirstOrDefaultAsync(x => x.FollowerUserId == sourceUserId && x.FollowsUserId == parameters.UserId);
		if (follow != null) {
			_context.Set<FollowEntity>().Remove(follow);
			await _context.SaveChangesAsync();
		}
		else {
			follow = new FollowEntity {
				FollowerUserId = sourceUserId,
				FollowsUserId = parameters.UserId
			};

			await _context.Set<FollowEntity>().AddAsync(follow);
			await _context.SaveChangesAsync();
			UserEntity? followsUser = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == parameters.UserId);
			if(followsUser!= null)
            {
				followsUser.Point = followsUser.Point + 1;
				_context.SaveChanges();
            }
			try {
				_notificationRepository.CreateNotification(new NotificationCreateUpdateDto {
					UserId = parameters.UserId, Message = "You are being followed by " + myUser.UserName, Title = "Follow",
					UseCase = "Follow", CreatorUserId = sourceUserId
				});
			}
			catch { }
		}

		return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
	}

	public async Task<GenericResponse> RemoveFollowings(string userId, FollowCreateDto parameters) {
		FollowEntity? following = await _context.Set<FollowEntity>()
			.Where(x => x.FollowerUserId == parameters.UserId && x.FollowsUserId == userId)
			.FirstOrDefaultAsync();
		if (following != null) {
			_context.Set<FollowEntity>().Remove(following);
			await _context.SaveChangesAsync();
		}

		return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
	}
}