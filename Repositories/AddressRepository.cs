namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid addressId, CancellationToken ct);
}

public class AddressRepository : IAddressRepository {
	private readonly DbContext _dbContext;
	private readonly IOutputCacheStore _outputCache;
	private readonly string? _userId;

	public AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IOutputCacheStore outputCache) {
		_dbContext = dbContext;
		_outputCache = outputCache;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

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
		await _dbContext.Set<AddressEntity>().AddAsync(e, ct);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse<AddressEntity?>(e);
	}

	public async Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto addressDto, CancellationToken ct) {
		AddressEntity e = (await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressDto.Id, ct))!;
		e.PostalCode = addressDto.PostalCode ?? e.PostalCode;
		e.Pelak = addressDto.Pelak ?? e.Pelak;
		e.Unit = addressDto.Unit ?? e.Unit;
		e.Address = addressDto.Address ?? e.Address;
		e.UpdatedAt = DateTime.Now;
		e.ReceiverFullName = addressDto.ReceiverFullName ?? e.ReceiverFullName;
		e.ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber ?? e.ReceiverPhoneNumber;
		if (addressDto.IsDefault.IsTrue() && _dbContext.Set<AddressEntity>().Any(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault))
			foreach (var item in _dbContext.Set<AddressEntity>().Where(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault)) {
				item.IsDefault = false;
				_dbContext.Update(item);
			}

		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync(ct);
		await _outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse<AddressEntity?>(e);
	}
	
	public async Task<GenericResponse> Delete(Guid addressId, CancellationToken ct) {
		await _dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteDeleteAsync(ct);
		await _outputCache.EvictByTagAsync("address", ct);
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto) {
		IQueryable<AddressEntity> q = _dbContext.Set<AddressEntity>().AsNoTracking();
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(o => o.UserId == dto.UserId);
		if (dto.OrderByIsDefault.IsTrue()) q = q.OrderBy(o => o.IsDefault);
		if (dto.OrderByPelak.IsTrue()) q = q.OrderBy(o => o.Pelak);
		if (dto.OrderByAddress.IsTrue()) q = q.OrderBy(o => o.Address);
		if (dto.OrderByPostalCode.IsTrue()) q = q.OrderBy(o => o.PostalCode);
		if (dto.OrderByReceiverFullName.IsTrue()) q = q.OrderBy(o => o.ReceiverFullName);
		if (dto.OrderByReceiverPhoneNumber.IsTrue()) q = q.OrderBy(o => o.ReceiverPhoneNumber);
		if (dto.OrderByUnit.IsTrue()) q = q.OrderBy(o => o.Unit);

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<AddressEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}