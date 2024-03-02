namespace Utilities_aspnet.Repositories;

public interface ICategoryRepository {
	public Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto, CancellationToken ct);
	public Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto, CancellationToken ct);
	public Task<GenericResponse> ImportFromExcel(IFormFile file, CancellationToken ct);
	public GenericResponse<IEnumerable<CategoryEntity>> Filter(CategoryFilterDto dto);
	public Task<GenericResponse<CategoryEntity?>> Update(CategoryCreateUpdateDto dto, CancellationToken ct);
	public Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class CategoryRepository(DbContext context, IMediaRepository mediaRepository) : ICategoryRepository {
	public async Task<GenericResponse<CategoryEntity>> Create(CategoryCreateUpdateDto dto, CancellationToken ct) {
		CategoryEntity entity = new();
		if (dto.Id is not null) entity.Id = (Guid)dto.Id;
		CategoryEntity i = entity.FillData(dto);
		if (dto.IsUnique) {
			CategoryEntity? exists = await context.Set<CategoryEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Title == dto.Title, ct);
			if (exists != null) return new GenericResponse<CategoryEntity>(exists);
			await context.AddAsync(i, ct);
			await context.SaveChangesAsync(ct);
			return new GenericResponse<CategoryEntity>(i);
		}

		await context.AddAsync(i, ct);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<CategoryEntity>(i);
	}

	public async Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateUpdateDto> dto, CancellationToken ct) {
		List<CategoryEntity> list = [];
		foreach (CategoryCreateUpdateDto i in dto) {
			GenericResponse<CategoryEntity> j = await Create(i, ct);
			list.Add(j.Result!);
		}

		return new GenericResponse<IEnumerable<CategoryEntity>>(list);
	}

	public async Task<GenericResponse> ImportFromExcel(IFormFile file, CancellationToken ct) {
		List<CategoryCreateUpdateDto> list = [];
		using MemoryStream stream = new();
		await file.CopyToAsync(stream, ct);
		using ExcelPackage package = new(stream);
		ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
		int rowCount = worksheet.Dimension.Rows;
		for (int i = 2; i < rowCount; i++) {
			list.Add(new CategoryCreateUpdateDto {
				Id = Guid.TryParse(worksheet.Cells[i, 0].Value.ToString(), out _) ? Guid.Parse(worksheet.Cells[i, 0].Value.ToString()!) : null,
				Title = worksheet.Cells[i, 2].Value.ToString(),
				TitleTr1 = worksheet.Cells[i, 3].Value.ToString(),
				ParentId = Guid.TryParse(worksheet.Cells[i, 4].Value.ToString(), out _) ? Guid.Parse(worksheet.Cells[i, 4].Value.ToString()!) : null,
				Tags = new List<TagCategory> { TagCategory.Category },
				IsUnique = false
			});
		}

		await BulkCreate(list, ct);

		return new GenericResponse();
	}

	public GenericResponse<IEnumerable<CategoryEntity>> Filter(CategoryFilterDto dto) {
		IQueryable<CategoryEntity> q = context.Set<CategoryEntity>().AsNoTracking().Include(x => x.Children);

		q = dto.ShowByChildren.IsTrue() ? q.Where(x => x.ParentId != null) : q.Where(x => x.ParentId == null);
		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title!.Contains(dto.Title!));
		if (dto.TitleTr1.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr1!.Contains(dto.TitleTr1!));
		if (dto.TitleTr2.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr2!.Contains(dto.TitleTr2!));
		if (dto.ParentId != null) q = q.Where(x => x.ParentId == dto.ParentId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedAtDescending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByOrder.IsTrue()) q = q.OrderBy(x => x.Order);
		if (dto.OrderByOrderDescending.IsTrue()) q = q.OrderByDescending(x => x.Order);
		if (dto.OrderByCreatedAtDescending.IsTrue()) q = q.OrderByDescending(x => x.Order);

		if (dto.ShowMedia.IsTrue()) q = q.Include(x => x.Media);

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IEnumerable<CategoryEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		CategoryEntity i = (await context.Set<CategoryEntity>().Include(x => x.Children).Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id, ct))!;
		foreach (CategoryEntity c in i.Children ?? new List<CategoryEntity>()) {
			context.Remove(c);
			await mediaRepository.DeleteMedia(c.Media);
		}

		context.Remove(i);
		await mediaRepository.DeleteMedia(i.Media);
		await context.SaveChangesAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<CategoryEntity?>> Update(CategoryCreateUpdateDto dto, CancellationToken ct) {
		CategoryEntity? entity = await context.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
		if (entity == null) return new GenericResponse<CategoryEntity?>(null, UtilitiesStatusCodes.NotFound);
		entity.FillData(dto);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<CategoryEntity?>(entity);
	}
}

public static class CategoryEntityExtension {
	public static CategoryEntity FillData(this CategoryEntity entity, CategoryCreateUpdateDto dto) {
		entity.Title = dto.Title ?? entity.Title;
		entity.TitleTr1 = dto.TitleTr1 ?? entity.TitleTr1;
		entity.TitleTr2 = dto.TitleTr2 ?? entity.TitleTr2;
		entity.UpdatedAt = DateTime.UtcNow;
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

		if (dto.RemoveTags.IsNotNullOrEmpty()) {
			dto.RemoveTags?.ForEach(item => entity.Tags.Remove(item));
		}

		if (dto.AddTags.IsNotNullOrEmpty()) {
			entity.Tags.AddRange(dto.AddTags!);
		}

		return entity;
	}
}