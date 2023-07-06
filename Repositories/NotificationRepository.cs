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
	private readonly string? _userId;

	public NotificationRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public GenericResponse<IQueryable<NotificationEntity>> Read() {
		IQueryable<NotificationEntity> i = _dbContext.Set<NotificationEntity>()
			.Include(x => x.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Media)
			.Include(x => x.CreatorUser).ThenInclude(x => x!.Categories)
			.Include(x => x.User)
			.Where(x => x.UserId == null || x.UserId == _userId)
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
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.OrderByDescending(x => x.CreatedAt);

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => (x.UserId ?? "").Contains(dto.UserId!));
		if (dto.CreatorUserId.IsNotNullOrEmpty()) q = q.Where(x => (x.CreatorUserId ?? "").Contains(dto.CreatorUserId!));
		if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => (x.UseCase ?? "").Contains(dto.UseCase!));
		if (dto.Message.IsNotNullOrEmpty()) q = q.Where(x => (x.Message ?? "").Contains(dto.Message!));
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags!.Any(y => dto.Tags!.Contains(y)));

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
			.Where(x => x.UserId == null || x.UserId == _userId)
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
			Tags = model.Tags,
			Visited = false
		};
		await _dbContext.Set<NotificationEntity>().AddAsync(notification);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse();
	}
}