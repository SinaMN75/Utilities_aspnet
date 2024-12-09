namespace Utilities_aspnet.Repositories;

public interface ICategoryRepository {
	public Task<GenericResponse<CategoryEntity>> Create(CategoryCreateDto dto, CancellationToken ct);
	public Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(IEnumerable<CategoryCreateDto> dto, CancellationToken ct);
	public Task<GenericResponse> ImportFromExcel(IFormFile file, CancellationToken ct);
	public Task<GenericResponse<IEnumerable<CategoryEntity>>> Filter(CategoryFilterDto dto);
	public Task<GenericResponse<CategoryEntity?>> Update(CategoryUpdateDto dto, CancellationToken ct);
	public Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class CategoryRepository(DbContext context, IMediaRepository mediaRepository) : ICategoryRepository {
	public async Task<GenericResponse<CategoryEntity>> Create(CategoryCreateDto dto, CancellationToken ct) {
		CategoryEntity entity = new() {
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Title = dto.Title,
			TitleTr1 = dto.TitleTr1,
			TitleTr2 = dto.TitleTr2,
			Order = dto.Order,
			Tags = dto.Tags,
			ParentId = dto.ParentId,
			JsonDetail = new CategoryJsonDetail {
				Subtitle = dto.Subtitle,
				Price = dto.Price,
				Color = dto.Color,
				Stock = dto.Stock,
				Link = dto.Link,
				Latitude = dto.Latitude,
				Longitude = dto.Longitude,
				Date1 = dto.Date1,
				Date2 = dto.Date2,
				Value = dto.Value,
				DiscountedPrice = dto.DiscountedPrice,
				SendPrice = dto.SendPrice
			}
		};
		await context.AddAsync(entity, ct);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<CategoryEntity>(entity);
	}

	public async Task<GenericResponse<IEnumerable<CategoryEntity>>> BulkCreate(
		IEnumerable<CategoryCreateDto> dto,
		CancellationToken ct
	) {
		List<CategoryEntity> list = [];
		foreach (CategoryCreateDto i in dto) {
			GenericResponse<CategoryEntity> j = await Create(i, ct);
			list.Add(j.Result!);
		}

		return new GenericResponse<IEnumerable<CategoryEntity>>(list);
	}

	public async Task<GenericResponse> ImportFromExcel(IFormFile file, CancellationToken ct) {
		List<CategoryCreateDto> list = [];
		using MemoryStream stream = new();
		await file.CopyToAsync(stream, ct);
		ExcelWorksheet worksheet = new ExcelPackage(stream).Workbook.Worksheets[0];
		for (int i = 2; i < worksheet.Dimension.Rows; i++) {
			list.Add(new CategoryCreateDto {
				Title = worksheet.Cells[i, 2].Value.ToString()!,
				TitleTr1 = worksheet.Cells[i, 3].Value.ToString(),
				ParentId = Guid.TryParse(worksheet.Cells[i, 4].Value.ToString(), out _) ? Guid.Parse(worksheet.Cells[i, 4].Value.ToString()!) : null,
				Tags = [TagCategory.Category]
			});
		}

		await BulkCreate(list, ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<IEnumerable<CategoryEntity>>> Filter(CategoryFilterDto dto) {
		IQueryable<CategoryEntity> q = context.Set<CategoryEntity>()
			.Where(x => x.ParentId == null)
			.Include(x => x.Children).AsNoTracking();

		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title!.Contains(dto.Title!));
		if (dto.TitleTr1.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr1!.Contains(dto.TitleTr1!));
		if (dto.TitleTr2.IsNotNullOrEmpty()) q = q.Where(x => x.TitleTr2!.Contains(dto.TitleTr2!));
		if (dto.ParentId != null) q = q.Where(x => x.ParentId == dto.ParentId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedAtDescending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByOrder.IsTrue()) q = q.OrderBy(x => x.Order);
		if (dto.OrderByOrderDescending.IsTrue()) q = q.OrderByDescending(x => x.Order);

		if (dto.ShowMedia.IsTrue()) q = q.Include(x => x.Media);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IEnumerable<CategoryEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		CategoryEntity i = (await context.Set<CategoryEntity>()
			.Include(x => x.Children)
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == id, ct))!;
		foreach (CategoryEntity c in i.Children ?? new List<CategoryEntity>()) {
			context.Remove(c);
			await mediaRepository.DeleteMedia(c.Media);
		}

		context.Remove(i);
		await mediaRepository.DeleteMedia(i.Media);
		await context.SaveChangesAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<CategoryEntity?>> Update(CategoryUpdateDto dto, CancellationToken ct) {
		CategoryEntity entity = (await context.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct))!;
		entity.UpdatedAt = DateTime.UtcNow;
		if (dto.Title is not null) entity.Title = dto.Title;
		if (dto.TitleTr1 is not null) entity.TitleTr1 = dto.TitleTr1;
		if (dto.TitleTr2 is not null) entity.TitleTr2 = dto.TitleTr2;
		if (dto.Tags is not null) entity.Tags = dto.Tags;
		if (dto.Order is not null) entity.Order = dto.Order;
		if (dto.Subtitle is not null) entity.JsonDetail.Subtitle = dto.Subtitle;
		if (dto.Price is not null) entity.JsonDetail.Price = dto.Price;
		if (dto.Color is not null) entity.JsonDetail.Color = dto.Color;
		if (dto.Stock is not null) entity.JsonDetail.Stock = dto.Stock;
		if (dto.Link is not null) entity.JsonDetail.Link = dto.Link;
		if (dto.Latitude is not null) entity.JsonDetail.Latitude = dto.Latitude;
		if (dto.Longitude is not null) entity.JsonDetail.Longitude = dto.Longitude;
		if (dto.Date1 is not null) entity.JsonDetail.Date1 = dto.Date1;
		if (dto.Date2 is not null) entity.JsonDetail.Date2 = dto.Date2;
		if (dto.Value is not null) entity.JsonDetail.Value = dto.Value;
		if (dto.DiscountedPrice is not null) entity.JsonDetail.DiscountedPrice = dto.DiscountedPrice;
		if (dto.SendPrice is not null) entity.JsonDetail.SendPrice = dto.SendPrice;
		await context.SaveChangesAsync(ct);
		return new GenericResponse<CategoryEntity?>(entity);
	}
}