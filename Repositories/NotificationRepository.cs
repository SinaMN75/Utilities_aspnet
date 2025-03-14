﻿namespace Utilities_aspnet.Repositories;

public interface INotificationRepository {
	Task<GenericResponse> Create(NotificationCreateUpdateDto model);
	Task<GenericResponse> Delete(Guid id);
	GenericResponse<IQueryable<NotificationEntity>> Filter(NotificationFilterDto dto);
	Task<GenericResponse> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus);
}

public class NotificationRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : INotificationRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse> Delete(Guid id) {
		NotificationEntity e = (await dbContext.Set<NotificationEntity>().FirstOrDefaultAsync(x => x.Id == id))!;
		dbContext.Remove(e);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<NotificationEntity>> Filter(NotificationFilterDto dto) {
		IQueryable<NotificationEntity> q = dbContext.Set<NotificationEntity>().AsNoTracking().OrderByDescending(x => x.CreatedAt);

		if (dto.ShowMedia.IsTrue()) q = q.Include(x => x.Media);
		if (dto.ShowCreator.IsTrue()) q = q.Include(x => x.CreatorUser).ThenInclude(x => x!.Media);
		if (dto.ShowUser.IsTrue()) q = q.Include(x => x.User).ThenInclude(x => x!.Media);
		if (dto.ShowProduct.IsTrue()) q = q.Include(x => x.Product).ThenInclude(x => x!.Media);
		if (dto.ShowComment.IsTrue()) {
			q = q.Include(x => x.Comment)
				.ThenInclude(x => x!.Product).ThenInclude(x => x!.Media);
			q = q.Include(x => x.Comment)
				.ThenInclude(x => x!.User).ThenInclude(x => x!.Media);
		}

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => (x.UserId ?? "").Contains(dto.UserId!));
		if (dto.CreatorUserId.IsNotNullOrEmpty()) q = q.Where(x => (x.CreatorUserId ?? "").Contains(dto.CreatorUserId!));
		if (dto.Message.IsNotNullOrEmpty()) q = q.Where(x => (x.Message ?? "").Contains(dto.Message!));
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.Any(y => x.Tags.Contains(y)));
		if (dto.SeenStatus.HasValue) q = q.Where(x => x.SeenStatus == dto.SeenStatus);

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<NotificationEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse> UpdateSeenStatus(IEnumerable<Guid> ids, SeenStatus seenStatus) {
		IQueryable<NotificationEntity> i = dbContext.Set<NotificationEntity>()
			.Where(x => x.UserId == null || x.UserId == _userId)
			.Where(x => ids.Contains(x.Id));

		foreach (NotificationEntity e in i) {
			e.SeenStatus = seenStatus;
			dbContext.Update(e);
		}

		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> Create(NotificationCreateUpdateDto model) {
		NotificationEntity notification = new() {
			Link = model.Link,
			Message = model.Message,
			Title = model.Title,
			UserId = model.UserId,
			CreatorUserId = model.CreatorUserId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			ProductId = model.ProductId,
			Tags = model.Tags ?? [],
			CommentId = model.CommentId,
			GroupChatId = model.GroupChatId
		};
		await dbContext.Set<NotificationEntity>().AddAsync(notification);
		await dbContext.SaveChangesAsync();

		return new GenericResponse();
	}
}