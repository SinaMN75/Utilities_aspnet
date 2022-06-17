namespace Utilities_aspnet.Report;

public interface IReportRepository {
    Task<GenericResponse<ReportReadDto?>> Create(ReportCreateDto dto);
    Task<GenericResponse<IEnumerable<ReportReadDto>>> Read(ReportFilterDto dto);
    Task<GenericResponse> Delete(Guid id);
}

public class ReportRepository : IReportRepository {
    private readonly DbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReportRepository(DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) {
        _dbContext = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponse<ReportReadDto?>> ReadById(Guid id) {
        ReportReadDto? report = await _dbContext.Set<ReportEntity>()
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Product)
            .Select(x => new ReportReadDto() {
                CreatedAt = x.CreatedAt,
                DeletedAt = x.DeletedAt,
                Description = x.Description,
                Id = x.Id,
                Product = x.Product,
                Title = x.Title,
                UpdatedAt = x.UpdatedAt,
                User = x.User
            })
            .FirstOrDefaultAsync();

        return new GenericResponse<ReportReadDto?>(report);
    }

    public async Task<GenericResponse<ReportReadDto?>> Create(ReportCreateDto dto) {
        ReportEntity entity = new() {
            UserId = _httpContextAccessor.HttpContext!.User.Identity!.Name!,
            Title = dto.Title,
            Description = dto.Description
        };

        if (dto.ProductId.HasValue) entity.ProductId = dto.ProductId;

        await _dbContext.Set<ReportEntity>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return await ReadById(entity.Id);
    }

    public async Task<GenericResponse<IEnumerable<ReportReadDto>>> Read(ReportFilterDto dto) {
        IQueryable<ReportEntity>? entities = _dbContext.Set<ReportEntity>().AsNoTracking();

        if (dto.User == true)
            entities = entities.Include(x => x.User);

        if (dto.Product == true)
            entities = entities.Include(x => x.Product);

        List<ReportReadDto>? result = await entities.Select(x => new ReportReadDto() {
            CreatedAt = x.CreatedAt,
            DeletedAt = x.DeletedAt,
            Description = x.Description,
            Id = x.Id,
            Product = x.Product,
            Title = x.Title,
            UpdatedAt = x.UpdatedAt,
            User = x.User
        }).ToListAsync();

        return new GenericResponse<IEnumerable<ReportReadDto>>(result);
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
}