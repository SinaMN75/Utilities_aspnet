namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity>> Update(ContentCreateUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class ContentRepository : IContentRepository {
	private readonly DbContext _dbContext;

	public ContentRepository(DbContext dbContext) => _dbContext = dbContext;

	public async Task<GenericResponse<ContentEntity>> Create(ContentCreateUpdateDto dto) {
		EntityEntry<ContentEntity> e = await _dbContext.Set<ContentEntity>().AddAsync(dto.Map());
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentEntity>(e.Entity);
	}

	public GenericResponse<IQueryable<ContentEntity>> Read() {
		IQueryable<ContentEntity> e = _dbContext.Set<ContentEntity>()
			.AsNoTracking()
			.Where(x => x.DeletedAt == null)
			.Select(x => new ContentEntity {
				Title = x.Title,
				SubTitle = x.SubTitle,
				Description = x.Description,
				Type = x.Type,
				UseCase = x.UseCase,
				Media = x.Media!.Select(y => new MediaEntity {
					Link = y.Link,
					UseCase = y.UseCase,
					Title = y.Title,
					FileName = y.FileName
				})
			});
		return new GenericResponse<IQueryable<ContentEntity>>(e);
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
		ContentEntity e = (await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(i => i.Id == id))!;
		e.DeletedAt = DateTime.Now;
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}