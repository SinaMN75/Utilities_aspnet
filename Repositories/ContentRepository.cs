namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity>> Update(ContentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class ContentRepository : IContentRepository {
	private readonly DbContext _dbContext;
	private readonly IOutputCacheStore _outputCache;

	public ContentRepository(DbContext dbContext, IOutputCacheStore cache) {
		_dbContext = dbContext;
		_outputCache = cache;
	}

	public async Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto, CancellationToken ct) {
		ContentEntity entity = new() {
			Description = dto.Description,
			Title = dto.Title,
			Type = dto.Type,
			SubTitle = dto.SubTitle,
			UseCase = dto.UseCase,
			Tags = dto.Tags,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};
		EntityEntry<ContentEntity> e = await _dbContext.Set<ContentEntity>().AddAsync(entity, ct);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("content", ct);
		return new GenericResponse<ContentEntity>(e.Entity);
	}

	public GenericResponse<IQueryable<ContentEntity>> Read() {
		return new GenericResponse<IQueryable<ContentEntity>>(_dbContext.Set<ContentEntity>().AsNoTracking().Include(x => x.Media));
	}

	public async Task<GenericResponse<ContentEntity>> Update(ContentCreateUpdateDto dto, CancellationToken ct) {
		ContentEntity e = (await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct))!;
		e.UseCase = dto.UseCase ?? e.UseCase;
		e.Title = dto.Title ?? e.Title;
		e.Type = dto.Type ?? e.Type;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		e.Tags = dto.Tags ?? e.Tags;
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("content", ct);
		return new GenericResponse<ContentEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		await _dbContext.Set<ContentEntity>().Where(i => i.Id == id).ExecuteDeleteAsync(ct);
		await _outputCache.EvictByTagAsync("content", ct);
		return new GenericResponse();
	}
}