namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto dto);
	GenericResponse<IQueryable<AddressEntity>> ReadMyAddresses();
	Task<GenericResponse> DeleteAddress(Guid addressId);
}

public class AddressRepository : IAddressRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto addressDto) {
		IQueryable<AddressEntity> userAddresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId);
		if (userAddresses.Any(a => a.PostalCode.Contains(addressDto.PostalCode) && a.Id != addressDto.Id))
			return new GenericResponse<AddressEntity?>(null, UtilitiesStatusCodes.BadRequest);

		AddressEntity e = new() {
			Address = addressDto.Address,
			CreatedAt = DateTime.UtcNow,
			Pelak = addressDto.Pelak,
			PostalCode = addressDto.PostalCode,
			ReceiverFullName = addressDto.ReceiverFullName,
			ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber,
			Unit = addressDto.Unit,
			UserId = _userId,
		};
		await _dbContext.Set<AddressEntity>().AddAsync(e);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<AddressEntity?>(e);
	}

	public async Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto addressDto) {
		IQueryable<AddressEntity> userAddresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId);
		if (userAddresses.Any(a => a.PostalCode.Contains(addressDto.PostalCode ?? string.Empty) && a.Id != addressDto.Id))
			return new GenericResponse<AddressEntity?>(null, UtilitiesStatusCodes.BadRequest);

		AddressEntity entity = await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressDto.Id && f.DeletedAt == null);
		if (entity is null) return new GenericResponse<AddressEntity?>(null, UtilitiesStatusCodes.NotFound);

		entity.PostalCode = addressDto.PostalCode ?? entity.PostalCode;
		entity.Pelak = addressDto.Pelak ?? entity.Pelak;
		entity.Unit = addressDto.Unit ?? entity.Unit;
		entity.Address = addressDto.Address ?? entity.Address;
		entity.UpdatedAt = DateTime.UtcNow;
		entity.ReceiverFullName = addressDto.ReceiverFullName ?? entity.ReceiverFullName;
		entity.ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber ?? entity.ReceiverPhoneNumber;

		_dbContext.Update(entity);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<AddressEntity?>(entity);
	}

	public GenericResponse<IQueryable<AddressEntity>> ReadMyAddresses() {
		IQueryable<AddressEntity> addresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId && w.DeletedAt == null);
		return new GenericResponse<IQueryable<AddressEntity>>(addresses);
	}

	public async Task<GenericResponse> DeleteAddress(Guid addressId) {
		AddressEntity? address = await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressId);
		if (address is null) return new GenericResponse(UtilitiesStatusCodes.NotFound);

		address.DeletedAt = DateTime.UtcNow;

		_dbContext.Update(address);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse();
	}
}