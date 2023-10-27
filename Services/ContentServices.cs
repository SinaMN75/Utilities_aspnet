namespace Utilities_aspnet.Services;

public interface IContentService {
	Task<ContentEntity> Create(ContentEntity entity, CancellationToken ct);
	IQueryable<ContentEntity> Read();
	Task<ContentEntity?> ReadById(Guid id);
	Task Update(ContentEntity dto, CancellationToken ct);
	Task Delete(Guid id, CancellationToken ct);
}

public class ContentService(DbContext dbContext) : IContentService {
	public async Task<ContentEntity> Create(ContentEntity entity, CancellationToken ct) {
		await dbContext.AddAsync(entity, ct);
		await dbContext.SaveChangesAsync(ct);
		return entity;
	}

	public IQueryable<ContentEntity> Read() => dbContext.Set<ContentEntity>().AsNoTracking();

	public async Task<ContentEntity?> ReadById(Guid id) =>
		await dbContext.Set<ContentEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

	public async Task Update(ContentEntity entity, CancellationToken ct) => await dbContext.Set<ContentEntity>()
		.Where(x => x.Id == entity.Id)
		.ExecuteUpdateAsync(s => s
			.SetProperty(x => x.Title, entity.Title)
			.SetProperty(x => x.SubTitle, entity.SubTitle)
			.SetProperty(x => x.Description, entity.Description)
			.SetProperty(x => x.JsonDetail, entity.JsonDetail)
			.SetProperty(x => x.Tags, entity.Tags)
			.SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

	public async Task Delete(Guid id, CancellationToken ct) =>
		await dbContext.Set<ContentEntity>().Where(x => x.Id == id).ExecuteDeleteAsync(ct);
}