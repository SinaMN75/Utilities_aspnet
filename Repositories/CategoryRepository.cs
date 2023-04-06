namespace Utilities_aspnet.Repositories;

public interface ICategoryRepository {
	public Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto);
	public GenericResponse<IQueryable<CategoryEntity>> Filter(CategoryFilterDto dto);
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
	
	public GenericResponse<IQueryable<CategoryEntity>> Filter(CategoryFilterDto dto) {
		IQueryable<CategoryEntity> q = _dbContext.Set<CategoryEntity>();
		q = q.Where(x => x.DeletedAt == null && x.ParentId == null);

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
		if (dto.Type.IsNotNullOrEmpty()) q = q.Where(x => (x.Type ?? "").Contains(dto.Type!));
		if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => x.UseCase.Contains(dto.UseCase));
		if (dto.TitleTr1.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr1.Contains(dto.TitleTr1));
		if (dto.TitleTr2.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr2.Contains(dto.TitleTr2));
		if (dto.TitleTr2.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr2.Contains(dto.TitleTr2));
		if (dto.ParentId != null) q = q.Where(x => x.ParentId == dto.ParentId);

		if (dto.ShowMedia.IsTrue()) q = q.Include(x => x.Media);

		q = q.Include(i => i.Children.Where(x => x.DeletedAt == null)).ThenInclude(i => i.Media.Where(i => i.DeletedAt == null));

		return new GenericResponse<IQueryable<CategoryEntity>>(q);
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
		entity.Subtitle = dto.Subtitle ?? entity.Subtitle;
		entity.Link = dto.Link ?? entity.Link;
		entity.Type = dto.Type ?? entity.Type;
		entity.Latitude = dto.Latitude ?? entity.Latitude;
		entity.Longitude = dto.Longitude ?? entity.Longitude;
		entity.UseCase = dto.UseCase ?? entity.UseCase;
		entity.Price = dto.Price ?? entity.Price;
		entity.DeletedAt = dto.DeletedAt ?? entity.DeletedAt;
		entity.UpdatedAt = DateTime.Now;
		entity.Date1 = dto.Date1 ?? entity.Date1;
		entity.Date2 = dto.Date2 ?? entity.Date2;
		entity.Color = dto.Color ?? entity.Color;
		entity.Value = dto.Value ?? entity.Value;
		entity.Stock = dto.Stock ?? entity.Stock;
		entity.ParentId = dto.ParentId ?? entity.ParentId;
		return entity;
	}
}