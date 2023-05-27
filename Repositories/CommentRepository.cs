namespace Utilities_aspnet.Repositories;

public interface ICommentRepository {
	Task<GenericResponse<CommentEntity?>> Create(CommentCreateUpdateDto dto);
	Task<GenericResponse> AddReactionToComment(Guid commentId, Reaction reaction);
	Task<GenericResponse<CommentEntity?>> Read(Guid id);
	GenericResponse<IQueryable<CommentEntity>?> ReadByProductId(Guid id);
	GenericResponse<IQueryable<CommentEntity>?> Filter(CommentFilterDto dto);
	Task<GenericResponse<CommentEntity?>> Update(Guid id, CommentCreateUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class CommentRepository : ICommentRepository {
	private readonly DbContext _dbContext;
	private readonly INotificationRepository _notificationRepository;
	private readonly IMediaRepository _mediaRepository;
	private readonly string? _userId;
	private readonly IConfiguration _config;

	public CommentRepository(
		DbContext dbContext,
		IHttpContextAccessor httpContextAccessor,
		INotificationRepository notificationRepository,
		IConfiguration config,
		IMediaRepository mediaRepository) {
		_dbContext = dbContext;
		_notificationRepository = notificationRepository;
		_config = config;
		_mediaRepository = mediaRepository;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public GenericResponse<IQueryable<CommentEntity>?> ReadByProductId(Guid id) {
		IQueryable<CommentEntity> comment = _dbContext.Set<CommentEntity>()
			.Include(x => x.Media)
			.Where(x => x.ProductId == id && x.ParentId == null && x.DeletedAt == null)
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Children.Where(x => x.DeletedAt == null)).ThenInclude(x => x.Media).Where(x => x.DeletedAt == null)
			.Include(x => x.Children.Where(x => x.DeletedAt == null)).ThenInclude(x => x.User).ThenInclude(x => x.Media)
			.Where(x => x.DeletedAt == null)
			.OrderByDescending(x => x.CreatedAt).AsNoTracking();
		return new GenericResponse<IQueryable<CommentEntity>?>(comment);
	}

	public GenericResponse<IQueryable<CommentEntity>?> Filter(CommentFilterDto dto) {
		IQueryable<CommentEntity> q = _dbContext.Set<CommentEntity>();
		if (!dto.ShowDeleted) q = q.Where(x => x.DeletedAt != null);

		if (dto.ProductId.HasValue) q = q.Where(x => x.ProductId == dto.ProductId);
		if (dto.Status.HasValue) q = q.Where(x => x.Status == dto.Status);
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == dto.UserId);

		q = q.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Media)
			.Include(x => x.Product).ThenInclude(x => x.Media)
			.Include(x => x.Children.Where(x => x.DeletedAt == null))!.ThenInclude(x => x.User).ThenInclude(x => x!.Media)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking();

		if (dto.ShowProducts.IsTrue()) q = q.Include(x => x.Product).ThenInclude(x => x.Media);

		return new GenericResponse<IQueryable<CommentEntity>?>(q);
	}

	public async Task<GenericResponse<CommentEntity?>> Read(Guid id) {
		CommentEntity? comment = await _dbContext.Set<CommentEntity>()
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.Media)
			.Include(x => x.Children.Where(x => x.DeletedAt == null)).ThenInclude(x => x.User).ThenInclude(x => x.Media)
			.Include(x => x.Children.Where(x => x.DeletedAt == null)).ThenInclude(x => x.Media)
			.Where(x => x.Id == id && x.DeletedAt == null)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking()
			.FirstOrDefaultAsync();

		return new GenericResponse<CommentEntity?>(comment);
	}

	public async Task<GenericResponse<CommentEntity?>> Create(CommentCreateUpdateDto dto) {
		AppSettings appSettings = new();
		_config.GetSection("AppSettings").Bind(appSettings);
		Tuple<bool, UtilitiesStatusCodes>? overUsedCheck =
			Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateComment, null, null, appSettings.UsageRules!);
		if (overUsedCheck.Item1)
			return new GenericResponse<CommentEntity?>(null, overUsedCheck.Item2);

		ProductEntity? prdct = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == dto.ProductId);
		if (prdct is not null) {
			Tuple<bool, UtilitiesStatusCodes>? blockedState = Utils.IsBlockedUser(_dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == prdct.UserId),
			                                                                      _dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == _userId));
			if (blockedState.Item1)
				return new GenericResponse<CommentEntity?>(null, blockedState.Item2);
		}

		CommentEntity comment = new() {
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			Comment = dto.Comment,
			ProductId = dto.ProductId,
			Score = dto.Score,
			ParentId = dto.ParentId,
			UserId = _userId,
			Status = dto.Status
		};
		await _dbContext.AddAsync(comment);
		await _dbContext.SaveChangesAsync();

		try {
			ProductEntity? product = _dbContext.Set<ProductEntity>()
				.Include(x => x.Media)
				.Include(x => x.User)
				.FirstOrDefault(x => x.Id == comment.ProductId);

			if (product != null && product.UserId != _userId) {
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
		}
		catch { }

		return await Read(comment.Id);
	}

	public async Task<GenericResponse<CommentEntity?>> Update(Guid id, CommentCreateUpdateDto dto) {
		CommentEntity? comment = await _dbContext.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == id);

		if (comment == null) return new GenericResponse<CommentEntity?>(null);
		if (!string.IsNullOrEmpty(dto.Comment)) comment.Comment = dto.Comment;
		if (dto.Score.HasValue) comment.Score = dto.Score;
		if (dto.ProductId.HasValue) comment.ProductId = dto.ProductId;
		if (dto.Status.HasValue) comment.Status = dto.Status;

		comment.UpdatedAt = DateTime.Now;
		_dbContext.Set<CommentEntity>().Update(comment);
		await _dbContext.SaveChangesAsync();

		return await Read(comment.Id);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		CommentEntity? comment = await _dbContext.Set<CommentEntity>().Include(i => i.Media).Include(i => i.Children).FirstOrDefaultAsync(x => x.Id == id);
		if (comment == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		foreach (MediaEntity i in comment.Media) await _mediaRepository.Delete(i.Id);
		foreach (CommentEntity i in comment.Children) await Delete(i.Id);
		_dbContext.Set<CommentEntity>().Remove(comment);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> AddReactionToComment(Guid commentId, Reaction reaction) {
		UserEntity? user = await _dbContext.Set<UserEntity>().Where(w => w.Id == _userId).FirstOrDefaultAsync();
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound, "User Donest Logged In");

		CommentEntity? comment = await _dbContext.Set<CommentEntity>().Where(w => w.Id == commentId).FirstOrDefaultAsync();
		if (comment is null) return new GenericResponse(UtilitiesStatusCodes.NotFound, "Comment Not Found");

		CommentReacts? oldReaction = comment.CommentJsonDetail.Reacts.FirstOrDefault(w => w.UserId == _userId);
		if (oldReaction is null) {
			CommentReacts? react = new() {
				Reaction = reaction,
				UserId = user.Id
			};
			comment.CommentJsonDetail.Reacts.Add(react);
		}
		else if (oldReaction.Reaction != reaction) {
			oldReaction.Reaction = reaction;
		}
		else {
			comment.CommentJsonDetail.Reacts.Remove(oldReaction);
		}
		await _dbContext.SaveChangesAsync();
		return new GenericResponse(UtilitiesStatusCodes.Success, "Ok");
	}
}