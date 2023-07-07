﻿namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity>> Update(ContentCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class ContentRepository : IContentRepository {
	private readonly DbContext _dbContext;
	private readonly IMediaRepository _mediaRepository;
	private readonly IOutputCacheStore _outputCache;

	public ContentRepository(DbContext dbContext, IOutputCacheStore cache, IMediaRepository mediaRepository) {
		_dbContext = dbContext;
		_outputCache = cache;
		_mediaRepository = mediaRepository;
	}

	public async Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto, CancellationToken ct) {
		ContentEntity entity = new() {
			Description = dto.Description,
			Title = dto.Title,
			SubTitle = dto.SubTitle,
			Tags = dto.Tags
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
		e.Title = dto.Title ?? e.Title;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		e.UpdatedAt = DateTime.Now;
		e.Tags = dto.Tags ?? e.Tags;
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("content", ct);
		return new GenericResponse<ContentEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ContentEntity e = (await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == id, ct))!;
		await _mediaRepository.DeleteMedia(e.Media);
		_dbContext.Set<ContentEntity>().Remove(e);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("content", ct);
		return new GenericResponse();
	}
}