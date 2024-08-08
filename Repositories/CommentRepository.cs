namespace Utilities_aspnet.Repositories;

public interface ICommentRepository {
	Task<GenericResponse<CommentEntity?>> Create(CommentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> AddReactionToComment(Guid commentId, Reaction reaction, CancellationToken ct);
	Task<GenericResponse<CommentEntity?>> ReadById(Guid id);
	Task<GenericResponse<IQueryable<CommentEntity>?>> Filter(CommentFilterDto dto);
	Task<GenericResponse<CommentEntity?>> Update(Guid id, CommentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class CommentRepository(
	DbContext dbContext,
	IHttpContextAccessor httpContextAccessor,
	INotificationRepository notificationRepository,
	IReportRepository reportRepository,
	IConfiguration config,
	IMediaRepository mediaRepository
)
	: ICommentRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<IQueryable<CommentEntity>?>> Filter(CommentFilterDto dto) {
		IQueryable<CommentEntity> q = dbContext.Set<CommentEntity>().AsNoTracking().Where(x => x.ParentId == null);

		if (dto.ProductId is not null) q = q.Where(x => x.ProductId == dto.ProductId);
		if (dto.Status is not null) q = q.Where(x => x.Status == dto.Status);
		if (dto.UserId is not null) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.ProductOwnerId is not null) q = q.Where(x => x.Product!.UserId == dto.ProductOwnerId);
		if (dto.Tags is not null) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));
		if (dto.TargetUserId is not null) q = q.Where(x => x.TargetUserId == dto.TargetUserId);
		if (dto.FromDate is not null) q = q.Where(x => x.CreatedAt > dto.FromDate);

		q = q.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Media)
			.Include(x => x.TargetUser)
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.OrderByDescending(x => x.CreatedAt);

		int totalCount = await dbContext.Set<CommentEntity>().AsNoTracking().Where(x => x.ProductId == dto.ProductId).CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<CommentEntity>?>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<CommentEntity?>> ReadById(Guid id) {
		CommentEntity? comment = await dbContext.Set<CommentEntity>()
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Media)
			.Include(x => x.TargetUser)
			.Include(x => x.Children)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Children)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Children)!.ThenInclude(x => x.Media)
			.Where(x => x.Id == id)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking()
			.FirstOrDefaultAsync();

		return new GenericResponse<CommentEntity?>(comment);
	}

	public async Task<GenericResponse<CommentEntity?>> Create(CommentCreateUpdateDto dto, CancellationToken ct) {
		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);
		
		CommentEntity comment = new() {
			Id = Guid.NewGuid(),
			Comment = dto.Comment,
			ProductId = dto.ProductId,
			TargetUserId = dto.UserId,
			Score = dto.Score,
			ParentId = dto.ParentId,
			UserId = _userId,
			Status = dto.Status,
			Tags = dto.Tags ?? []
		};
		await dbContext.AddAsync(comment, ct);

		if (dto.ProductId is not null) {
			ProductEntity product = (await dbContext.Set<ProductEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ProductId, ct))!;
			product.CommentsCount += 1;
			
			if (product.UserId != _userId)
				await notificationRepository.Create(new NotificationCreateUpdateDto {
					UserId = product.UserId,
					Message = dto.Comment ?? "",
					Title = "Comment",
					CreatorUserId = _userId,
					Link = product.Id.ToString(),
					CommentId = comment.Id,
					Tags = [TagNotification.ReceivedComment],
					ProductId = product.Id
				});
		}
		
		if (dto.UserId is not null) {
			if (dto.UserId != _userId)
				await notificationRepository.Create(new NotificationCreateUpdateDto {
					UserId = dto.UserId,
					Message = dto.Comment ?? "",
					Title = "Comment",
					CreatorUserId = comment.UserId,
					CommentId = comment.Id,
					Tags = [TagNotification.ReceivedComment],
				});
		}

		await dbContext.SaveChangesAsync(ct);
		return await ReadById(comment.Id);
	}

	public async Task<GenericResponse<CommentEntity?>> Update(Guid id, CommentCreateUpdateDto dto, CancellationToken ct) {
		CommentEntity? comment = await dbContext.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);

		if (comment == null) return new GenericResponse<CommentEntity?>(null);
		if (!string.IsNullOrEmpty(dto.Comment)) comment.Comment = dto.Comment;
		if (dto.Score.HasValue) comment.Score = dto.Score;
		if (dto.ProductId.HasValue) comment.ProductId = dto.ProductId;
		if (dto.UserId.IsNotNullOrEmpty()) comment.TargetUserId = dto.UserId;
		if (dto.Status.HasValue) comment.Status = dto.Status;
		if (dto.Tags.IsNotNullOrEmpty()) comment.Tags = dto.Tags!;
		if (dto.RemoveTags.IsNotNullOrEmpty()) {
			dto.RemoveTags!.ForEach(item => comment.Tags.Remove(item));
		}

		if (dto.AddTags.IsNotNullOrEmpty()) comment.Tags.AddRange(dto.AddTags!);

		comment.UpdatedAt = DateTime.UtcNow;
		dbContext.Set<CommentEntity>().Update(comment);
		await dbContext.SaveChangesAsync(ct);
		return await ReadById(comment.Id);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		CommentEntity? comment = await dbContext.Set<CommentEntity>()
			.Include(i => i.Media)
			.Include(i => i.Reports)
			.Include(i => i.Children)!.ThenInclude(i => i.Notifications)
			.Include(i => i.Children)!.ThenInclude(i => i.Media)
			.Include(i => i.Notifications)
			.FirstOrDefaultAsync(x => x.Id == id, ct);
		if (comment == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		foreach (MediaEntity i in comment.Media!) await mediaRepository.Delete(i.Id);
		foreach (CommentEntity i in comment.Children!) await Delete(i.Id, ct);
		foreach (NotificationEntity i in comment.Notifications!) await notificationRepository.Delete(i.Id);
		foreach (ReportEntity i in comment.Reports!) await reportRepository.Delete(i.Id);
		dbContext.Set<CommentEntity>().Remove(comment);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse> AddReactionToComment(Guid commentId, Reaction reaction, CancellationToken ct) {
		UserEntity user = (await dbContext.Set<UserEntity>().Where(w => w.Id == _userId).FirstOrDefaultAsync(ct))!;

		CommentEntity? comment = await dbContext.Set<CommentEntity>().Where(w => w.Id == commentId).FirstOrDefaultAsync(ct);
		if (comment is null) return new GenericResponse(UtilitiesStatusCodes.NotFound, "Comment Not Found");

		CommentReacts? oldReaction = comment.JsonDetail.Reacts.FirstOrDefault(w => w.UserId == _userId);
		if (oldReaction is null) {
			comment.JsonDetail.Reacts.Add(new CommentReacts { Reaction = reaction, UserId = user.Id });
			if (comment.UserId != _userId) {
				await notificationRepository.Create(new NotificationCreateUpdateDto {
					UserId = comment.UserId,
					Tags = [TagNotification.ReceivedReactionOnComment],
					CommentId = comment.Id,
					CreatorUserId = _userId
				});
			}
		}
		else if (oldReaction.Reaction != reaction) {
			oldReaction.Reaction = reaction;
		}
		else {
			comment.JsonDetail.Reacts.Remove(oldReaction);
		}

		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}
}