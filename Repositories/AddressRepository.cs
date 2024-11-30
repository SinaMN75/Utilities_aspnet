namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class AddressRepository(DbContext context) : IAddressRepository {
	public async Task<GenericResponse<AddressEntity?>> Create(AddressCreateDto dto, CancellationToken ct) {
		EntityEntry<AddressEntity> e = await context.Set<AddressEntity>().AddAsync(new AddressEntity {
			Address = dto.Address,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Pelak = dto.Pelak,
			PostalCode = dto.PostalCode,
			ReceiverFullName = dto.ReceiverFullName,
			ReceiverPhoneNumber = dto.ReceiverPhoneNumber,
			Unit = dto.Unit,
			UserId = dto.UserId
		}, ct);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<AddressEntity?>(e.Entity);
	}

	public async Task<GenericResponse<AddressEntity?>> Update(AddressUpdateDto dto, CancellationToken ct) {
		AddressEntity e = (await context.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (dto.PostalCode is not null) e.PostalCode = dto.PostalCode!;
		if (dto.Pelak is not null) e.Pelak = dto.Pelak!;
		if (dto.Unit is not null) e.Unit = dto.Unit!;
		if (dto.Address is not null) e.Address = dto.Address!;
		if (dto.ReceiverFullName is not null) e.ReceiverFullName = dto.ReceiverFullName!;
		if (dto.ReceiverPhoneNumber is not null) e.ReceiverPhoneNumber = dto.ReceiverPhoneNumber!;

		context.Update(e);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<AddressEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		await context.Set<AddressEntity>().Where(f => f.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto) {
		IQueryable<AddressEntity> q = context.Set<AddressEntity>().AsNoTracking().Select(x => new AddressEntity {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			ReceiverFullName = x.ReceiverFullName,
			ReceiverPhoneNumber = x.ReceiverPhoneNumber,
			Address = x.Address,
			Pelak = x.Pelak,
			Unit = x.Unit,
			PostalCode = x.PostalCode,
			UserId = x.UserId
		});

		if (dto.UserId is not null) q = q.Where(o => o.UserId == dto.UserId);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<AddressEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}