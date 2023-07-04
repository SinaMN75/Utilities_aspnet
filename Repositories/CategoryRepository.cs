namespace Utilities_aspnet.Repositories;

public interface ICategoryRepository {
	public Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto, CancellationToken ct);
	public Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto, CancellationToken ct);
	public GenericResponse<IEnumerable<CategoryEntity>> Filter(CategoryFilterDto dto);
	public Task<GenericResponse<CategoryEntity?>> Update(CategoryCreateUpdateDto dto, CancellationToken ct);
	public Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class CategoryRepository : ICategoryRepository {
	private readonly DbContext _dbContext;
	private readonly IOutputCacheStore _outputCache;

	public CategoryRepository(DbContext context, IOutputCacheStore outputCache) {
		_dbContext = context;
		_outputCache = outputCache;
	}

	public async Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto, CancellationToken ct) {
		CategoryEntity entity = new();
		if (dto.Id is not null) entity.Id = (Guid) dto.Id;
			CategoryEntity i = entity.FillData(dto);
			await _outputCache.EvictByTagAsync("category", ct);
			if (dto.IsUnique) {
			CategoryEntity? exists =
				await _dbContext.Set<CategoryEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Title == dto.Title && x.DeletedAt != null, ct);
			if (exists == null) {
				await _dbContext.AddAsync(i, ct);
				await _dbContext.SaveChangesAsync(ct);
				return new GenericResponse<CategoryEntity>(i);
			}
			return new GenericResponse<CategoryEntity>(exists);
		}
		{
			await _dbContext.AddAsync(i, ct);
			await _dbContext.SaveChangesAsync(ct);
			return new GenericResponse<CategoryEntity>(i);
		}
	}

	public async Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto, CancellationToken ct) {
		List<CategoryEntity> list = new();
		foreach (CategoryCreateUpdateDto i in dto) {
			GenericResponse<CategoryEntity> j = await Create(i, ct);
			list.Add(j.Result!);
		}
		return new GenericResponse<IEnumerable<CategoryEntity>>(list);
	}

	public GenericResponse<IEnumerable<CategoryEntity>> Filter(CategoryFilterDto dto) {
		IQueryable<CategoryEntity> q = _dbContext.Set<CategoryEntity>().AsNoTracking()
			.Where(x => x.DeletedAt == null && x.ParentId == null)
			.Include(x => x.Children!.Where(y => y.DeletedAt == null));

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title!.Contains(dto.Title!));
		if (dto.Type.IsNotNullOrEmpty()) q = q.Where(x => x.Type!.Contains(dto.Type!));
		if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => x.UseCase!.Contains(dto.UseCase!));
		if (dto.TitleTr1.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr1!.Contains(dto.TitleTr1!));
		if (dto.TitleTr2.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr2!.Contains(dto.TitleTr2!));
		if (dto.ParentId != null) q = q.Where(x => x.ParentId == dto.ParentId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags!.Any(y => dto.Tags!.Contains(y)));
		
		if (dto.OrderByOrder.IsTrue()) q = q.OrderBy(x => x.Order);
		if (dto.OrderByOrderDescending.IsTrue()) q = q.OrderByDescending(x => x.Order);
		if (dto.OrderByCreatedAtDescending.IsTrue()) q = q.OrderByDescending(x => x.Order);
		if (dto.OrderByCreatedAt.IsTrue()) q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedAtDescending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		if (dto.ShowMedia.IsTrue()) q = q.Include(x => x.Media);

		return new GenericResponse<IEnumerable<CategoryEntity>>(q.AsNoTracking());
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		await _dbContext.Set<CategoryEntity>().Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, DateTime.Now), ct);
		await _outputCache.EvictByTagAsync("category", ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<CategoryEntity?>> Update(CategoryCreateUpdateDto dto, CancellationToken ct) {
		CategoryEntity? entity = await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
		if (entity == null) return new GenericResponse<CategoryEntity?>(null, UtilitiesStatusCodes.NotFound);
		entity.FillData(dto);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("category", ct);
		return new GenericResponse<CategoryEntity?>(entity);
	}
}

public static class CategoryEntityExtension {
	public static CategoryEntity FillData(this CategoryEntity entity, CategoryCreateUpdateDto dto) {
		entity.Title = dto.Title ?? entity.Title;
		entity.TitleTr1 = dto.TitleTr1 ?? entity.TitleTr1;
		entity.TitleTr2 = dto.TitleTr2 ?? entity.TitleTr2;
		entity.UseCase = dto.UseCase ?? entity.UseCase;
		entity.UpdatedAt = DateTime.Now;
		entity.Type = dto.Type ?? entity.Type;
		entity.Order = dto.Order ?? entity.Order;
		entity.ParentId = dto.ParentId ?? entity.ParentId;
		entity.Tags = dto.Tags ?? entity.Tags;
		entity.JsonDetail = new CategoryJsonDetail {
			Subtitle = dto.Subtitle ?? entity.JsonDetail.Subtitle,
			Price = dto.Price ?? entity.JsonDetail.Price,
			Color = dto.Color ?? entity.JsonDetail.Color,
			Stock = dto.Stock ?? entity.JsonDetail.Stock,
			Link = dto.Link ?? entity.JsonDetail.Link,
			Latitude = dto.Latitude ?? entity.JsonDetail.Latitude,
			Longitude = dto.Longitude ?? entity.JsonDetail.Longitude,
			Date1 = dto.Date1 ?? entity.JsonDetail.Date1,
			Date2 = dto.Date2 ?? entity.JsonDetail.Date2,
			Value = dto.Value ?? entity.JsonDetail.Value,
			DiscountedPrice = dto.DiscountedPrice ?? entity.JsonDetail.DiscountedPrice,
			SendPrice = dto.SendPrice ?? entity.JsonDetail.SendPrice
		};

		return entity;
	}
}