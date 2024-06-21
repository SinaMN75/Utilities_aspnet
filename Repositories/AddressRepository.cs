namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : IAddressRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<AddressEntity?>> Create(AddressCreateDto addressDto, CancellationToken ct) {
		EntityEntry<AddressEntity> e = await dbContext.Set<AddressEntity>().AddAsync(new AddressEntity {
			Address = addressDto.Address,
			CreatedAt = DateTime.UtcNow,
			Pelak = addressDto.Pelak,
			PostalCode = addressDto.PostalCode,
			ReceiverFullName = addressDto.ReceiverFullName,
			ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber,
			Unit = addressDto.Unit,
			UserId = _userId,
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<AddressEntity?>(e.Entity);
	}

	public async Task<GenericResponse<AddressEntity?>> Update(AddressUpdateDto dto, CancellationToken ct) {
		AddressEntity e = (await dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (dto.PostalCode.IsNotNullOrEmpty()) e.PostalCode = dto.PostalCode!;
		if (dto.Pelak.IsNotNullOrEmpty()) e.Pelak = dto.Pelak!;
		if (dto.Unit.IsNotNullOrEmpty()) e.Unit = dto.Unit!;
		if (dto.Address.IsNotNullOrEmpty()) e.Address = dto.Address!;
		if (dto.ReceiverFullName.IsNotNullOrEmpty()) e.ReceiverFullName = dto.ReceiverFullName!;
		if (dto.ReceiverPhoneNumber.IsNotNullOrEmpty()) e.ReceiverPhoneNumber = dto.ReceiverPhoneNumber!;

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<AddressEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid addressId, CancellationToken ct) {
		await dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<IQueryable<AddressEntity>>> Filter(AddressFilterDto dto) {
		IQueryable<AddressEntity> q = dbContext.Set<AddressEntity>().AsNoTracking().Select(x => new AddressEntity {
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
		
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(o => o.UserId == dto.UserId);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<AddressEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}