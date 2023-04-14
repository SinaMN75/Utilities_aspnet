namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentReadDto>> Create(ContentCreateUpdateDto dto);
	GenericResponse<IQueryable<ContentReadDto>> Read();
	Task<GenericResponse<ContentReadDto>> Update(ContentCreateUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class ContentRepository : IContentRepository {
	private readonly DbContext _dbContext;

	public ContentRepository(DbContext dbContext) => _dbContext = dbContext;

	public async Task<GenericResponse<ContentReadDto>> Create(ContentCreateUpdateDto dto) {
		EntityEntry<ContentEntity> e = await _dbContext.Set<ContentEntity>().AddAsync(dto.Map());
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentReadDto>(e.Entity.Map());
	}

	public GenericResponse<IQueryable<ContentReadDto>> Read() {
		IQueryable<ContentReadDto> e = _dbContext.Set<ContentEntity>()
			.AsNoTracking()
			.Where(x => x.DeletedAt == null)
			.Select(x => new ContentReadDto {
				Title = x.Title,
				SubTitle = x.SubTitle,
				Description = x.Description,
				Type = x.Type,
				UseCase = x.UseCase,
				Media = x.Media!.Select(y => new MediaReadDto {
					Link = y.Link,
					UseCase = y.UseCase,
					Title = y.Title,
					FileName = y.FileName
				})
			});
		return new GenericResponse<IQueryable<ContentReadDto>>(e);
	}

	public async Task<GenericResponse<ContentReadDto>> Update(ContentCreateUpdateDto dto) {
		ContentEntity? e = await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);
		e.UseCase = dto.UseCase ?? e.UseCase;
		e.Title = dto.Title ?? e.Title;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentReadDto>(e.Map());
	}

	public async Task<GenericResponse> Delete(Guid id) {
		ContentEntity? e = await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(i => i.Id == id);
		e.DeletedAt = DateTime.Now;
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}