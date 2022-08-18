namespace Utilities_aspnet.Repositories;

public interface IReportRepository {
	Task<GenericResponse<ReportEntity?>> Create(ReportEntity dto);
	Task<GenericResponse<IEnumerable<ReportEntity>>> Read(ReportFilterDto dto);
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
		if (!dto.UserId.IsNotNullOrEmpty()) entity.UserId = dto.UserId;

		await _dbContext.Set<ReportEntity>().AddAsync(entity);
		await _dbContext.SaveChangesAsync();

		return await ReadById(entity.Id);
	}

	public async Task<GenericResponse<IEnumerable<ReportEntity>>> Read(ReportFilterDto dto) {
		IQueryable<ReportEntity> entities = _dbContext.Set<ReportEntity>().AsNoTracking();

		if (dto.User == true)
			entities = entities.Include(x => x.User).ThenInclude(x => x!.Media);

		if (dto.Product == true)
			entities = entities.Include(x => x.Product).ThenInclude(x => x!.Media);

		IEnumerable<ReportEntity> result = await entities.ToListAsync();

		return new GenericResponse<IEnumerable<ReportEntity>>(result);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		ReportEntity? report = await _dbContext.Set<ReportEntity>()
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == id);

		if (report == null)
			return new GenericResponse(UtilitiesStatusCodes.NotFound, "Report notfound");

		_dbContext.Set<ReportEntity>().Remove(report);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse(UtilitiesStatusCodes.Success, "Mission Accomplished");
	}

	public async Task<GenericResponse<ReportEntity?>> ReadById(Guid id) {
		ReportEntity? entity = await _dbContext.Set<ReportEntity>().AsNoTracking()
			.Include(x => x.User)
			.Include(x => x.Product)
			.FirstOrDefaultAsync(x => x.Id == id);

		return new GenericResponse<ReportEntity?>(entity);
	}
}