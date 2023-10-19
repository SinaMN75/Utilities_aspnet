namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid addressId, CancellationToken ct);
}

public class AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IOutputCacheStore outputCache) : IAddressRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto addressDto, CancellationToken ct) {
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

	public async Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto addressDto, CancellationToken ct) {
		AddressEntity e = (await dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressDto.Id, ct))!;
		e.PostalCode = addressDto.PostalCode;
		e.Pelak = addressDto.Pelak;
		e.Unit = addressDto.Unit;
		e.Address = addressDto.Address;
		e.UpdatedAt = DateTime.Now;
		e.ReceiverFullName = addressDto.ReceiverFullName;
		e.ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber;
		if (addressDto.IsDefault && dbContext.Set<AddressEntity>().Any(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault))
			foreach (AddressEntity? item in dbContext.Set<AddressEntity>().Where(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault)) {
				item.IsDefault = false;
				dbContext.Update(item);
			}

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		await outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse<AddressEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid addressId, CancellationToken ct) {
		await dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteDeleteAsync(ct);
		await outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse();
	}

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