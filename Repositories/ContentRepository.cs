namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentEntity dto);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity>> Update(ContentEntity dto);
	Task<GenericResponse> Delete(Guid id);
}

public class ContentRepository : IContentRepository {
	private readonly DbContext _dbContext;

	public ContentRepository(DbContext dbContext) => _dbContext = dbContext;

	public async Task<GenericResponse<ContentEntity>> Create(ContentEntity dto) {
		EntityEntry<ContentEntity> i = await _dbContext.Set<ContentEntity>().AddAsync(dto);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentEntity>(i.Entity);
	}

	public GenericResponse<IQueryable<ContentEntity>> Read() => new(_dbContext.Set<ContentEntity>()
		                                                                .Where(x => x.DeletedAt == null)
		                                                                .Include(x => x.Media).AsNoTracking());

	public async Task<GenericResponse<ContentEntity>> Update(ContentEntity dto) {
		ContentEntity? e = await _dbContext.Set<ContentEntity>().Where(x => x.Id == dto.Id).FirstOrDefaultAsync();
		if (e == null) return new GenericResponse<ContentEntity>(new ContentEntity());
		e.UseCase = dto.UseCase ?? e.UseCase;
		e.Title = dto.Title ?? e.Title;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<ContentEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		ContentEntity? e = await _dbContext.Set<ContentEntity>().FirstOrDefaultAsync(i => i.Id == id);
		e.DeletedAt = DateTime.Now;
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}