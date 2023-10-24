namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid addressId, CancellationToken ct);
}

public class AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IOutputCacheStore outputCache) : IAddressRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	[Time]
	public async Task<GenericResponse<AddressEntity?>> Create(AddressCreateDto addressDto, CancellationToken ct) {
		AddressEntity e = new() {
			Address = addressDto.Address,
			CreatedAt = DateTime.Now,
			Pelak = addressDto.Pelak,
			PostalCode = addressDto.PostalCode,
			ReceiverFullName = addressDto.ReceiverFullName,
			ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber,
			Unit = addressDto.Unit,
			UserId = _userId
		};
		await dbContext.Set<AddressEntity>().AddAsync(e, ct);
		await dbContext.SaveChangesAsync(ct);
		await outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse<AddressEntity?>(e);
	}

	[Time]
	public async Task<GenericResponse<AddressEntity?>> Update(AddressUpdateDto dto, CancellationToken ct) {
		AddressEntity e = (await dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		if (dto.PostalCode.IsNotNullOrEmpty()) e.UpdatedAt = DateTime.Now;
		if (dto.PostalCode.IsNotNullOrEmpty()) e.PostalCode = dto.PostalCode!;
		if (dto.Pelak.IsNotNullOrEmpty()) e.Pelak = dto.Pelak!;
		if (dto.Unit.IsNotNullOrEmpty()) e.Unit = dto.Unit!;
		if (dto.Address.IsNotNullOrEmpty()) e.Address = dto.Address!;
		if (dto.ReceiverFullName.IsNotNullOrEmpty()) e.ReceiverFullName = dto.ReceiverFullName!;
		if (dto.ReceiverPhoneNumber.IsNotNullOrEmpty()) e.ReceiverPhoneNumber = dto.ReceiverPhoneNumber!;
		if (dto.IsDefault && dbContext.Set<AddressEntity>().Any(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault))
			foreach (AddressEntity? item in dbContext.Set<AddressEntity>().Where(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault)) {
				item.IsDefault = false;
				dbContext.Update(item);
			}

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		await outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse<AddressEntity?>(e);
	}

	[Time]
	public async Task<GenericResponse> Delete(Guid addressId, CancellationToken ct) {
		await dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteDeleteAsync(ct);
		await outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse();
	}

	[Time]
	public GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto) {
		IQueryable<AddressEntity> q = dbContext.Set<AddressEntity>().AsNoTracking();
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(o => o.UserId == dto.UserId);

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<AddressEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}