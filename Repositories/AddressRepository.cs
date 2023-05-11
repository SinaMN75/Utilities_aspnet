namespace Utilities_aspnet.Repositories;

public interface IAddressRepository
{
    Task<GenericResponse<AddressEntity>> Create(AddressCreateUpdateDto UserAddressDto);
    Task<GenericResponse<AddressEntity>> Update(AddressCreateUpdateDto UserAddressDto);
    Task<GenericResponse<IEnumerable<AddressEntity>>> GetMyAddresses();
    Task<GenericResponse> DeleteAddress(Guid addressId);
}

public class AddressRepository : IAddressRepository
{
    private readonly DbContext _dbContext;
    private readonly string? _userId;
    public AddressRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
    }

    public async Task<GenericResponse<AddressEntity>> Create(AddressCreateUpdateDto addressDto)
    {
        var userAddresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId).ToList();
        if (userAddresses.Any(a => a.PostalCode.Contains(addressDto.PostalCode) && a.Id != addressDto.Id))
            return new GenericResponse<AddressEntity>(null, UtilitiesStatusCodes.BadRequest);

        AddressEntity? e = new AddressEntity
        {
            Address = addressDto.Address,
            CreatedAt = DateTime.UtcNow,
            Pelak = addressDto.Pelak,
            PostalCode = addressDto.PostalCode,
            RecieverFullName = addressDto.RecieverFullName,
            RecieverPhoneNumber = addressDto.RecieverPhoneNumber,
            Unit = addressDto.Unit,
            UserId = _userId,
        };
        await _dbContext.Set<AddressEntity>().AddAsync(e);
        await _dbContext.SaveChangesAsync();

        return new GenericResponse<AddressEntity>(e, UtilitiesStatusCodes.Success);
    }

    public async Task<GenericResponse<AddressEntity>> Update(AddressCreateUpdateDto addressDto)
    {
        var userAddresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId).ToList();
        if (userAddresses.Any(a => a.PostalCode.Contains(addressDto.PostalCode ?? string.Empty) && a.Id != addressDto.Id))
            return new GenericResponse<AddressEntity>(null, UtilitiesStatusCodes.BadRequest);


        AddressEntity entity = await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressDto.Id && f.DeletedAt == null);
        if (entity is null) return new GenericResponse<AddressEntity>(null, UtilitiesStatusCodes.NotFound);

        entity.PostalCode = addressDto.PostalCode ?? entity.PostalCode;
        entity.Pelak = addressDto.Pelak ?? entity.Pelak;
        entity.Unit = addressDto.Unit ?? entity.Unit;
        entity.Address = addressDto.Address ?? entity.Address;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.RecieverFullName = addressDto.RecieverFullName ?? entity.RecieverFullName;
        entity.RecieverPhoneNumber = addressDto.RecieverPhoneNumber ?? entity.RecieverPhoneNumber;

        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();

        return new GenericResponse<AddressEntity>(entity, UtilitiesStatusCodes.Success);
    }

    public async Task<GenericResponse<IEnumerable<AddressEntity>>> GetMyAddresses()
    {
        var addresses = _dbContext.Set<AddressEntity>().Where(w => w.UserId == _userId && w.DeletedAt == null);
        return new GenericResponse<IEnumerable<AddressEntity>>(addresses);
    }

    public async Task<GenericResponse> DeleteAddress(Guid addressId)
    {
        var address = await _dbContext.Set<AddressEntity>().FirstOrDefaultAsync(f => f.Id == addressId);
        if (address is null) return new GenericResponse(UtilitiesStatusCodes.NotFound);

        address.DeletedAt = DateTime.UtcNow;

        _dbContext.Update(address);
        await _dbContext.SaveChangesAsync();

        return new GenericResponse(UtilitiesStatusCodes.Success);
    }
}
