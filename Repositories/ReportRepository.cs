namespace Utilities_aspnet.Repositories;

public interface IReportRepository {
	Task<GenericResponse<ReportEntity?>> Create(ReportEntity dto);
	GenericResponse<IQueryable<ReportEntity>> Read(ReportFilterDto dto);
	Task<GenericResponse<ReportEntity?>> ReadById(Guid id);
	Task<GenericResponse> Delete(Guid id);
}

public class ReportRepository : IReportRepository {
	private readonly DbContext _dbContext;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public ReportRepository(DbContext context, IHttpContextAccessor httpContextAccessor) {
		_dbContext = context;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<GenericResponse<ReportEntity?>> Create(ReportEntity dto) {
		ReportEntity entity = new() {
			CreatorUserId = _httpContextAccessor.HttpContext!.User.Identity!.Name!,
			Title = dto.Title,
			Description = dto.Description
		};
		if (dto.ProductId.HasValue) entity.ProductId = dto.ProductId;
		if (dto.CommentId.HasValue) entity.CommentId = dto.CommentId;
		if (dto.ChatId.HasValue) entity.ChatId = dto.ChatId;
		if (dto.GroupChatId.HasValue) entity.GroupChatId = dto.GroupChatId;
		if (dto.GroupChatMessageId.HasValue) entity.GroupChatMessageId = dto.GroupChatMessageId;
		if (!dto.UserId.IsNotNullOrEmpty()) entity.UserId = dto.UserId;
		await _dbContext.Set<ReportEntity>().AddAsync(entity);
		await _dbContext.SaveChangesAsync();
		return await ReadById(entity.Id);
	}

	public GenericResponse<IQueryable<ReportEntity>> Read(ReportFilterDto dto) {
		IQueryable<ReportEntity> e = _dbContext.Set<ReportEntity>().AsNoTracking();
		if (dto.User == true) e = e.Include(x => x.User).ThenInclude(x => x!.Media);
		if (dto.Product == true) e = e.Include(x => x.Product).ThenInclude(x => x!.Media);
		if (dto.Chat == true) e = e.Include(x => x.Chat);
		if (dto.GroupChat == true) e = e.Include(x => x.GroupChat);
		if (dto.GroupChatMessage == true) e = e.Include(x => x.GroupChatMessage);
		return new GenericResponse<IQueryable<ReportEntity>>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		ReportEntity? report = await _dbContext.Set<ReportEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		if (report == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		_dbContext.Set<ReportEntity>().Remove(report);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<ReportEntity?>> ReadById(Guid id) {
		ReportEntity? entity = await _dbContext.Set<ReportEntity>()
			.Include(x => x.User)
			.Include(x => x.Product).ThenInclude(x => x.Media)
			.Include(x => x.Comment)
			.Include(x => x.GroupChat)
			.Include(x => x.Chat)
			.Include(x => x.GroupChatMessage)
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == id);
		return new GenericResponse<ReportEntity?>(entity);
	}
}