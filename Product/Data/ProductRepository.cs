using Microsoft.EntityFrameworkCore.ChangeTracking;
using Utilities_aspnet.Product.Dto;
using Utilities_aspnet.Utilities.Responses;

namespace Utilities_aspnet.Product.Data;

public interface IProductRepository<T> where T : BasePEntity {
    Task<GenericResponse<GetProductDto>> Add(AddUpdateProductDto dto);
    Task<GenericResponse<IEnumerable<GetProductDto>>> Get();
    Task<GenericResponse<GetProductDto>> GetById(Guid id);
    Task<GenericResponse<GetProductDto>> Update(Guid id, AddUpdateProductDto dto);
    void Delete(Guid id);
}

public class ProductRepository<T> : IProductRepository<T> where T : BasePEntity {
    private readonly DbContext _dbContext;
    private readonly IMapper _mapper;

    public ProductRepository(DbContext dbContext, IMapper mapper) {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GenericResponse<GetProductDto>> Add(AddUpdateProductDto dto) {
        if (dto == null) throw new ArgumentException("Dto must not be null", nameof(dto));
        EntityEntry<T> i = await _dbContext.Set<T>().AddAsync(_mapper.Map<T>(dto));
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<GetProductDto>(_mapper.Map<GetProductDto>(i.Entity));
    }

    public async Task<GenericResponse<IEnumerable<GetProductDto>>> Get() {
        List<T> i = await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        return new GenericResponse<IEnumerable<GetProductDto>>(_mapper.Map<IEnumerable<GetProductDto>>(i));
    }

    public async Task<GenericResponse<GetProductDto>> GetById(Guid id) {
        T? i = await _dbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return new GenericResponse<GetProductDto>(_mapper.Map<GetProductDto>(i));
    }

    public Task<GenericResponse<GetProductDto>> Update(Guid id, AddUpdateProductDto dto) {
        throw new NotImplementedException();
    }

    public async void Delete(Guid id) {
        GenericResponse<GetProductDto> i = await GetById(id);
        _dbContext.Set<T>().Remove(_mapper.Map<T>(i.Result));
        await _dbContext.SaveChangesAsync();
    }
}