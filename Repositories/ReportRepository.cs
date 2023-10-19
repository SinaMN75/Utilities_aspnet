namespace Utilities_aspnet.Repositories;

public interface IReportRepository {
	Task<GenericResponse<ReportEntity?>> Create(ReportCreateUpdateDto dto);
	GenericResponse<IQueryable<ReportEntity>> Read(ReportFilterDto dto);
	Task<GenericResponse<ReportEntity?>> ReadById(Guid id);
	Task<GenericResponse> Delete(Guid id);
}

public class ReportRepository(DbContext context, IHttpContextAccessor httpContextAccessor) : IReportRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	[Time]
	public async Task<GenericResponse<ReportEntity?>> Create(ReportCreateUpdateDto dto) {
		ReportEntity entity = new() {
			CreatorUserId = _userId,
			Title = dto.Title,
			Description = dto.Description
		};
		if (dto.ProductId.HasValue) entity.ProductId = dto.ProductId;
		if (dto.CommentId.HasValue) entity.CommentId = dto.CommentId;
		if (dto.GroupChatId.HasValue) entity.GroupChatId = dto.GroupChatId;
		if (dto.GroupChatMessageId.HasValue) entity.GroupChatMessageId = dto.GroupChatMessageId;
		if (!dto.UserId.IsNotNullOrEmpty()) entity.UserId = dto.UserId;
		await context.Set<ReportEntity>().AddAsync(entity);
		await context.SaveChangesAsync();
		return await ReadById(entity.Id);
	}

	[Time]
	public GenericResponse<IQueryable<ReportEntity>> Read(ReportFilterDto dto) {
		IQueryable<ReportEntity> e = context.Set<ReportEntity>().AsNoTracking();
		if (dto.User == true) e = e.Include(x => x.User).ThenInclude(x => x!.Media);
		if (dto.Product == true) e = e.Include(x => x.Product).ThenInclude(x => x!.Media);
		if (dto.GroupChat == true) e = e.Include(x => x.GroupChat);
		if (dto.GroupChatMessage == true) e = e.Include(x => x.GroupChatMessage);
		return new GenericResponse<IQueryable<ReportEntity>>(e);
	}

	[Time]
	public async Task<GenericResponse> Delete(Guid id) {
		await context.Set<ReportEntity>().Where(x => x.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	[Time]
	public async Task<GenericResponse<ReportEntity?>> ReadById(Guid id) {
		ReportEntity? entity = await context.Set<ReportEntity>()
			.Include(x => x.User)
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Comment)
			.Include(x => x.GroupChat)
			.Include(x => x.GroupChatMessage)
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == id);
		return new GenericResponse<ReportEntity?>(entity);
	}
}