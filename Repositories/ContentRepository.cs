﻿namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity>> Update(ContentCreateUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class ContentRepository : IContentRepository {
	private readonly DbContext _dbContext;

	public ContentRepository(DbContext dbContext) { _dbContext = dbContext; }

	public async Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto) {
		ContentEntity entity = new() {
			Description = dto.Description,
			Title = dto.Title,
			Type = dto.Type,
			SubTitle = dto.SubTitle,
			UseCase = dto.UseCase,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};
		EntityEntry<ContentEntity> e = await _dbContext.Set<ContentEntity>().AddAsync(entity);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentEntity>(e.Entity);
	}

	public GenericResponse<IQueryable<ContentEntity>> Read() {
		return new GenericResponse<IQueryable<ContentEntity>>(_dbContext.Set<ContentEntity>().AsNoTracking().Include(x => x.Media));
	}

	public async Task<GenericResponse<ContentEntity>> Update(ContentCreateUpdateDto dto) {
		ContentEntity e = (await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id))!;
		e.UseCase = dto.UseCase ?? e.UseCase;
		e.Title = dto.Title ?? e.Title;
		e.Type = dto.Type ?? e.Type;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await _dbContext.Set<ContentEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}
}