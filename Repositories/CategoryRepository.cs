namespace Utilities_aspnet.Repositories;

public interface ICategoryRepository {
	public Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto);
	public Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto);
	public GenericResponse<IEnumerable<CategoryEntity>> Filter(CategoryFilterDto dto);
	public Task<GenericResponse<CategoryEntity?>> Update(CategoryCreateUpdateDto dto);
	public Task<GenericResponse> Delete(Guid id);
}

public class CategoryRepository : ICategoryRepository {
	private readonly DbContext _dbContext;

	public CategoryRepository(DbContext context) => _dbContext = context;

	public async Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto) {
		CategoryEntity entity = new();
		CategoryEntity i = entity.FillData(dto);
		if (dto.IsUnique) {
			CategoryEntity? exists =
				await _dbContext.Set<CategoryEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Title == dto.Title && x.DeletedAt != null);
			if (exists == null) {
				await _dbContext.AddAsync(i);
				await _dbContext.SaveChangesAsync();
				return new GenericResponse<CategoryEntity>(i);
			}
			return new GenericResponse<CategoryEntity>(exists);
		}
		{
			await _dbContext.AddAsync(i);
			await _dbContext.SaveChangesAsync();
			return new GenericResponse<CategoryEntity>(i);
		}
	}

	public async Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto) {
		List<CategoryEntity> list = new();
		foreach (CategoryCreateUpdateDto i in dto) {
			GenericResponse<CategoryEntity> j = await Create(i);
			list.Add(j.Result);
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

		if (dto.OrderByOrder.IsTrue()) q = q.OrderBy(x => x.Order);
		if (dto.OrderByOrderDecending.IsTrue()) q = q.OrderByDescending(x => x.Order);
		if (dto.OrderByCreatedAt.IsTrue()) q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedAtDecending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		if (dto.ShowMedia.IsTrue()) q = q.Include(x => x.Media);

		return new GenericResponse<IEnumerable<CategoryEntity>>(q.AsNoTracking());
	}

	public async Task<GenericResponse> Delete(Guid id) {
		CategoryEntity? e = await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (e == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		e.DeletedAt = DateTime.Now;
		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<CategoryEntity?>> Update(CategoryCreateUpdateDto dto) {
		CategoryEntity? entity = await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);
		if (entity == null) return new GenericResponse<CategoryEntity?>(null, UtilitiesStatusCodes.NotFound);
		entity.FillData(dto);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<CategoryEntity?>(entity);
	}
}

public static class CategoryEntityExtension {
	public static CategoryEntity FillData(this CategoryEntity entity, CategoryCreateUpdateDto dto) {
		entity.Title = dto.Title ?? entity.Title;
		entity.TitleTr1 = dto.TitleTr1 ?? entity.TitleTr1;
		entity.TitleTr2 = dto.TitleTr2 ?? entity.TitleTr2;
		entity.CategoryJsonDetail.Subtitle = dto.Subtitle ?? entity.CategoryJsonDetail.Subtitle;
		entity.CategoryJsonDetail.Link = dto.Link ?? entity.CategoryJsonDetail.Link;
		entity.Type = dto.Type ?? entity.Type;
		entity.CategoryJsonDetail.Latitude = dto.Latitude ?? entity.CategoryJsonDetail.Latitude;
		entity.CategoryJsonDetail.Longitude = dto.Longitude ?? entity.CategoryJsonDetail.Longitude;
		entity.UseCase = dto.UseCase ?? entity.UseCase;
		entity.Price = dto.Price ?? entity.Price;
		entity.UpdatedAt = DateTime.Now;
		entity.CategoryJsonDetail.Date1 = dto.Date1 ?? entity.CategoryJsonDetail.Date1;
		entity.CategoryJsonDetail.Date2 = dto.Date2 ?? entity.CategoryJsonDetail.Date2;
		entity.Color = dto.Color ?? entity.Color;
		entity.CategoryJsonDetail.Value = dto.Value ?? entity.CategoryJsonDetail.Value;
		entity.Order = dto.Order ?? entity.Order;
		entity.Stock = dto.Stock ?? entity.Stock;
		entity.ParentId = dto.ParentId ?? entity.ParentId;
		return entity;
	}
}