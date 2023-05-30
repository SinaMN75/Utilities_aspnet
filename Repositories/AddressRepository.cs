namespace Utilities_aspnet.Repositories;

public interface IAddressRepository {
	Task<GenericResponse<AddressEntity?>> Create(AddressCreateUpdateDto dto);
	Task<GenericResponse<AddressEntity?>> Update(AddressCreateUpdateDto dto);
	GenericResponse<IQueryable<AddressEntity>> ReadMyAddresses();
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
		IQueryable<AddressEntity> userAddresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId);
		if (userAddresses.Any(a => a.PostalCode!.Contains(addressDto.PostalCode ?? string.Empty) && a.Id != addressDto.Id))
			return new GenericResponse<AddressEntity?>(null, UtilitiesStatusCodes.BadRequest);

		AddressEntity? e = await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressDto.Id && f.DeletedAt == null);
		if (e is null) return new GenericResponse<AddressEntity?>(null, UtilitiesStatusCodes.NotFound);

		e.PostalCode = addressDto.PostalCode ?? e.PostalCode;
		e.Pelak = addressDto.Pelak ?? e.Pelak;
		e.Unit = addressDto.Unit ?? e.Unit;
		e.Address = addressDto.Address ?? e.Address;
		e.UpdatedAt = DateTime.UtcNow;
		e.ReceiverFullName = addressDto.ReceiverFullName ?? e.ReceiverFullName;
		e.ReceiverPhoneNumber = addressDto.ReceiverPhoneNumber ?? e.ReceiverPhoneNumber;

		_dbContext.Update(e);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<AddressEntity?>(e);
	}

	public GenericResponse<IQueryable<AddressEntity>> ReadMyAddresses() =>
		new(_dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId && w.DeletedAt == null).AsNoTracking());

	public async Task<GenericResponse> Delete(Guid addressId) {
		await _dbContext.Set<AddressEntity>().Where(f => f.Id == addressId).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, DateTime.Now));
		return new GenericResponse();
	}
}