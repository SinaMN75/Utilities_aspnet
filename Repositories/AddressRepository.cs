namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto dto);
	GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto);
	Task<GenericResponse> Delete(Guid addressId);
}

public class AddressRepository : IAddressRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto addressDto) {
		AddressEntity e = new() {
			Address = addressDto.Address,
			CreatedAt = DateTime.UtcNow,
			Pelak = addressDto.Pelak,
			PostalCode = addressDto.PostalCode,
			ReceiverFullName = addressDto.ReceiverFullName,
			ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber,
			Unit = addressDto.Unit,
			UserId = _userId
		};
		await _dbContext.Set<AddressEntity>().AddAsync(e);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<AddressEntity?>(e);
	}

	public async Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto addressDto) {
		AddressEntity e = (await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressDto.Id && f.DeletedAt == null))!;
		e.PostalCode = addressDto.PostalCode ?? e.PostalCode;
		e.Pelak = addressDto.Pelak ?? e.Pelak;
		e.Unit = addressDto.Unit ?? e.Unit;
		e.Address = addressDto.Address ?? e.Address;
		e.UpdatedAt = DateTime.UtcNow;
		e.ReceiverFullName = addressDto.ReceiverFullName ?? e.ReceiverFullName;
		e.ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber ?? e.ReceiverPhoneNumber;
		if (addressDto.IsDefault.IsTrue() && _dbContext.Set<AddressEntity>().Any(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault))
			foreach (var item in _dbContext.Set<AddressEntity>().Where(a => a.UserId == e.UserId && a.Id != e.Id && e.IsDefault)) {
				item.IsDefault = false;
				_dbContext.Update(item);
			}

		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<AddressEntity?>(e);
	}
	
	public async Task<GenericResponse> Delete(Guid addressId) {
		await _dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, DateTime.Now));
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<AddressEntity>> Filter(AddressFilterDto dto) {
		IQueryable<AddressEntity> q = _dbContext.Set<AddressEntity>().AsNoTracking().Where(x => x.DeletedAt == null);
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