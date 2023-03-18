namespace Utilities_aspnet.Repositories;

public interface INotificationRepository {
	Task<GenericResponse> Create(NotificationCreateUpdateDto model);
	GenericResponse<IQueryable<NotificationEntity>> Read();
	GenericResponse<IQueryable<NotificationEntity>> Filter(NotificationFilterDto dto);
	Task<GenericResponse<NotificationEntity?>> ReadById(Guid id);
	Task<GenericResponse> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus);
}

public class NotificationRepository : INotificationRepository {
	private readonly DbContext _dbContext;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public NotificationRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_httpContextAccessor = httpContextAccessor;
	}

	public GenericResponse<IQueryable<NotificationEntity>> Read() {
		IQueryable<NotificationEntity> i = _dbContext.Set<NotificationEntity>()
			.Include(x => x.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Categories)
			.Include(x => x.User)
			.Where(x => (x.UserId == null || x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name) && x.DeletedAt == null)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking()
			.Take(100);

		return new GenericResponse<IQueryable<NotificationEntity>>(i);
	}

	public GenericResponse<IQueryable<NotificationEntity>> Filter(NotificationFilterDto dto) {
		IQueryable<NotificationEntity> q = _dbContext.Set<NotificationEntity>()
			.Include(x => x.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Categories)
			.OrderByDescending(x => x.CreatedAt);

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => (x.UserId ?? "").Contains(dto.UserId!));
		if (dto.CreatorUserId.IsNotNullOrEmpty()) q = q.Where(x => (x.CreatorUserId ?? "").Contains(dto.CreatorUserId!));
		if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => (x.UseCase ?? "").Contains(dto.UseCase!));
		if (dto.Message.IsNotNullOrEmpty()) q = q.Where(x => (x.Message ?? "").Contains(dto.Message!));

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).AsNoTracking();

		return new GenericResponse<IQueryable<NotificationEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<NotificationEntity?>> ReadById(Guid id) {
		NotificationEntity? e = await _dbContext.Set<NotificationEntity>().AsNoTracking()
			.Include(x => x.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Categories)
			.FirstOrDefaultAsync(i => i.Id == id);
		return e == null ? new GenericResponse<NotificationEntity?>(null, UtilitiesStatusCodes.NotFound) : new GenericResponse<NotificationEntity?>(e);
	}

	public async Task<GenericResponse> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus) {
		IQueryable<NotificationEntity> i = _dbContext.Set<NotificationEntity>()
			.Include(x => x.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Categories)
			.Where(x => (x.UserId == null || x.UserId == _httpContextAccessor.HttpContext!.User.Identity!.Name) && x.DeletedAt == null)
			.Where(x => ids.Contains(x.Id))
			.OrderByDescending(x => x.CreatedAt);

		foreach (NotificationEntity e in i) {
			e.SeenStatus = seenStatus;
			_dbContext.Update(e);
		}
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> Create(NotificationCreateUpdateDto model) {
		NotificationEntity notification = new() {
			UseCase = model.UseCase,
			Link = model.Link,
			Message = model.Message,
			Title = model.Title,
			UserId = model.UserId,
			CreatorUserId = model.CreatorUserId,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			Visited = false
		};
		await _dbContext.Set<NotificationEntity>().AddAsync(notification);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse();
	}
}