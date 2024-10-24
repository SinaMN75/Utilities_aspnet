namespace Utilities_aspnet.Repositories;

public interface IRegistrationRepository {
	Task<GenericResponse<RegistrationEntity?>> Create(RegistrationCreateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<RegistrationEntity>>> Filter(RegistrationFilterDto dto);
	Task<GenericResponse<RegistrationEntity?>> Update(RegistrationUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class RegistrationRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : IRegistrationRepository {

	public async Task<GenericResponse<RegistrationEntity?>> Create(RegistrationCreateDto dto, CancellationToken ct) {
		EntityEntry<RegistrationEntity> e = await dbContext.Set<RegistrationEntity>().AddAsync(new RegistrationEntity {
			ProductId = dto.ProductId,
			UserId = dto.UserId,
			Title = dto.Title,
			Subtitle = dto.Subtitle,
			Column = dto.Column,
			Row = dto.Row
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<RegistrationEntity?>(e.Entity);
	}

	public async Task<GenericResponse<RegistrationEntity?>> Update(RegistrationUpdateDto dto, CancellationToken ct) {
		RegistrationEntity e = (await dbContext.Set<RegistrationEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (dto.Title is not null) e.Title = dto.Title!;
		if (dto.Subtitle is not null) e.Subtitle = dto.Subtitle!;
		if (dto.Row is not null) e.Row = dto.Row!;
		if (dto.Column is not null) e.Column = dto.Column!;

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<RegistrationEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		await dbContext.Set<RegistrationEntity>().Where(f => f.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<IQueryable<RegistrationEntity>>> Filter(RegistrationFilterDto dto) {
		IQueryable<RegistrationEntity> q = dbContext.Set<RegistrationEntity>().AsNoTracking().Select(x => new RegistrationEntity {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			UserId = x.UserId,
			ProductId = x.ProductId,
			Title = x.Title,
			Subtitle = x.Subtitle,
			Column = x.Column,
			Row = x.Row,
			User = new UserEntity {
				Id = x.User.Id,
				Tags = x.User.Tags,
				JsonDetail = x.User.JsonDetail,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				FullName = x.User.FullName,
				AppUserName = x.User.AppUserName,
				AppEmail = x.User.AppEmail,
				AppPhoneNumber = x.User.AppPhoneNumber,
				Email = x.User.Email,
				PhoneNumber = x.User.PhoneNumber
			},
			Product = new ProductEntity {
				Id = x.Product!.Id,
				Title = x.Product.Title,
				Tags = x.Product.Tags,
				JsonDetail = x.Product.JsonDetail,
				ParentId = x.Product.ParentId,
				Media = x.Product.Media!.Select(y => new MediaEntity {
					Id = y.Id,
					FileName = y.FileName,
					Order = y.Order,
					JsonDetail = y.JsonDetail,
					Tags = y.Tags
				})
			}
		});

		if (dto.UserId is not null) q = q.Where(o => o.UserId == dto.UserId);
		if (dto.ProductId is not null) q = q.Where(o => o.ProductId == dto.ProductId);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<RegistrationEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}