namespace Utilities_aspnet.Repositories;

public interface ICommentRepository {
	Task<GenericResponse<CommentEntity?>> Create(CommentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> AddReactionToComment(Guid commentId, Reaction reaction, CancellationToken ct);
	Task<GenericResponse<CommentEntity?>> ReadById(Guid id);
	GenericResponse<IQueryable<CommentEntity>?> ReadByProductId(Guid id);
	GenericResponse<IQueryable<CommentEntity>?> Filter(CommentFilterDto dto);
	Task<GenericResponse<CommentEntity?>> Update(Guid id, CommentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class CommentRepository : ICommentRepository {
	private readonly IConfiguration _config;
	private readonly DbContext _dbContext;
	private readonly IMediaRepository _mediaRepository;
	private readonly INotificationRepository _notificationRepository;
	private readonly IOutputCacheStore _outputCache;
	private readonly string? _userId;

	public CommentRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor,
		INotificationRepository notificationRepository,
		IConfiguration config,
		IMediaRepository mediaRepository,
		IOutputCacheStore outputCache) {
		_dbContext = dbContext;
		_notificationRepository = notificationRepository;
		_config = config;
		_mediaRepository = mediaRepository;
		_outputCache = outputCache;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public GenericResponse<IQueryable<CommentEntity>?> ReadByProductId(Guid id) {
		IQueryable<CommentEntity> comment = _dbContext.Set<CommentEntity>()
			.Include(x => x.Media)
			.Where(x => x.ProductId == id && x.ParentId == null)
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.OrderByDescending(x => x.CreatedAt).AsNoTracking();
		return new GenericResponse<IQueryable<CommentEntity>?>(comment);
	}

	public GenericResponse<IQueryable<CommentEntity>?> Filter(CommentFilterDto dto) {
		IQueryable<CommentEntity> q = _dbContext.Set<CommentEntity>();

		q = q.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Media)
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking();

		if (dto.ProductId is not null) q = q.Where(x => x.ProductId == dto.ProductId);
		if (dto.Status is not null) q = q.Where(x => x.Status == dto.Status);
		if (dto.UserId is not null) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.ProductOwnerId is not null) q = q.Where(x => x.Product!.UserId == dto.ProductOwnerId);
		if (dto.Tags is not null) q = q.Where(x => dto.Tags!.All(y => x.Tags!.Contains(y)));

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<CommentEntity>?>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
		;
	}

	public async Task<GenericResponse<CommentEntity?>> ReadById(Guid id) {
		CommentEntity? comment = await _dbContext.Set<CommentEntity>()
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children)!.ThenInclude(x => x.Media)
			.Where(x => x.Id == id)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking()
			.FirstOrDefaultAsync();

		return new GenericResponse<CommentEntity?>(comment);
	}

	public async Task<GenericResponse<CommentEntity?>> Create(CommentCreateUpdateDto dto, CancellationToken ct) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		Tuple<bool, UtilitiesStatusCodes> overUsedCheck =
			Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateComment, null, null, appSettings.UsageRules);
		if (overUsedCheck.Item1) return new GenericResponse<CommentEntity?>(null, overUsedCheck.Item2);

		ProductEntity? prdct = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == dto.ProductId, ct);
		if (prdct is not null) {
			Tuple<bool, UtilitiesStatusCodes> blockedState = Utils.IsBlockedUser(_dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == prdct.UserId),
				_dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == _userId));
			if (blockedState.Item1) return new GenericResponse<CommentEntity?>(null, blockedState.Item2);
		}

		CommentEntity comment = new() {
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			Comment = dto.Comment,
			ProductId = dto.ProductId,
			Score = dto.Score,
			ParentId = dto.ParentId,
			UserId = _userId,
			Status = dto.Status,
			Tags = dto.Tags
		};
		await _dbContext.AddAsync(comment, ct);
		try {
			ProductEntity product = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == comment.ProductId, ct))!;
			product.CommentsCount += 1;

			if (product.UserId != _userId)
				await _notificationRepository.Create(new NotificationCreateUpdateDto {
					UserId = product.UserId,
					Message = dto.Comment ?? "",
					Title = "Comment",
					UseCase = "Comment",
					CreatorUserId = comment.UserId,
					Link = product.Id.ToString(),
					ProductId = product.Id
				});
		}
		catch { }

		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("comment", ct);
		return await ReadById(comment.Id);
	}

	public async Task<GenericResponse<CommentEntity?>> Update(Guid id, CommentCreateUpdateDto dto, CancellationToken ct) {
		CommentEntity? comment = await _dbContext.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);

		if (comment == null) return new GenericResponse<CommentEntity?>(null);
		if (!string.IsNullOrEmpty(dto.Comment)) comment.Comment = dto.Comment;
		if (dto.Score.HasValue) comment.Score = dto.Score;
		if (dto.ProductId.HasValue) comment.ProductId = dto.ProductId;
		if (dto.Status.HasValue) comment.Status = dto.Status;
		if (dto.Tags.IsNotNullOrEmpty()) comment.Tags = dto.Tags;
		if (dto.RemoveTags.IsNotNullOrEmpty()) {
			dto.RemoveTags.ForEach(item => comment.Tags?.Remove(item));
		}

		if (dto.AddTags.IsNotNullOrEmpty()) {
			comment.Tags.AddRange(dto.AddTags);
		}

		comment.UpdatedAt = DateTime.Now;
		_dbContext.Set<CommentEntity>().Update(comment);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("comment", ct);
		return await ReadById(comment.Id);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		CommentEntity? comment = await _dbContext.Set<CommentEntity>().Include(i => i.Media).Include(i => i.Children).FirstOrDefaultAsync(x => x.Id == id, ct);
		if (comment == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		foreach (MediaEntity i in comment.Media!) await _mediaRepository.Delete(i.Id);
		foreach (CommentEntity i in comment.Children!) await Delete(i.Id, ct);
		_dbContext.Set<CommentEntity>().Remove(comment);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("comment", ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse> AddReactionToComment(Guid commentId, Reaction reaction, CancellationToken ct) {
		UserEntity? user = await _dbContext.Set<UserEntity>().Where(w => w.Id == _userId).FirstOrDefaultAsync(ct);
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound, "User Donest Logged In");

		CommentEntity? comment = await _dbContext.Set<CommentEntity>().Where(w => w.Id == commentId).FirstOrDefaultAsync(ct);
		if (comment is null) return new GenericResponse(UtilitiesStatusCodes.NotFound, "Comment Not Found");

		CommentReacts? oldReaction = comment.JsonDetail.Reacts.FirstOrDefault(w => w.UserId == _userId);
		if (oldReaction is null) comment.JsonDetail.Reacts.Add(new CommentReacts { Reaction = reaction, UserId = user.Id });
		else if (oldReaction.Reaction != reaction)
			oldReaction.Reaction = reaction;
		else
			comment.JsonDetail.Reacts.Remove(oldReaction);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("comment", ct);
		return new GenericResponse();
	}
}