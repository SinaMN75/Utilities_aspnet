namespace Utilities_aspnet.Repositories;

public interface IVoteRepository {
	Task<GenericResponse> CreateUpdateVote(VoteCreateUpdateDto dto);
	Task<GenericResponse<IEnumerable<VoteReadDto>?>> CreateUpdateVoteFields(VoteFieldCreateUpdateDto dto);
	Task<GenericResponse<IEnumerable<VoteReadDto>?>> ReadVoteFields(Guid id);
}

public class VoteRepository : IVoteRepository {
	private readonly DbContext _dbContext;
	private readonly IMapper _mapper;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public VoteRepository(DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_mapper = mapper;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<GenericResponse<IEnumerable<VoteReadDto>?>> CreateUpdateVoteFields(VoteFieldCreateUpdateDto dto) {
		foreach (VoteFieldDto item in dto.VoteFields)
			try {
				VoteFieldEntity? up = await _dbContext.Set<VoteFieldEntity>().FirstOrDefaultAsync(x =>
					x.ProductId == dto.ProductId && x.Id == item.Id);
				if (up != null) {
					up.Title = item.Title;
					await _dbContext.SaveChangesAsync();
				}
				else {
					_dbContext.Set<VoteFieldEntity>().Add(new VoteFieldEntity {
						ProductId = dto.ProductId,
						Title = item.Title
					});
				}

				await _dbContext.SaveChangesAsync();
			}
			catch {
				// ignored
			}

		IEnumerable<VoteFieldEntity> entity =
			await _dbContext.Set<VoteFieldEntity>().Where(x => x.ProductId == dto.ProductId).ToListAsync();

		return new GenericResponse<IEnumerable<VoteReadDto>>(_mapper.Map<IEnumerable<VoteReadDto>>(entity));
	}

	public async Task<GenericResponse<IEnumerable<VoteReadDto>?>> ReadVoteFields(Guid id) {
		IEnumerable<VoteFieldEntity> entity = await _dbContext.Set<VoteFieldEntity>().Where(x => x.ProductId == id)
			.Include(x => x.Votes).ToListAsync();

		return new GenericResponse<IEnumerable<VoteReadDto>>(_mapper.Map<IEnumerable<VoteReadDto>>(entity));
	}

	public async Task<GenericResponse> CreateUpdateVote(VoteCreateUpdateDto dto) {
		string? userId = _httpContextAccessor.HttpContext?.User.Identity?.Name;
		foreach (VoteDto item in dto.Votes)
			try {
				VoteEntity? up = await _dbContext.Set<VoteEntity>().FirstOrDefaultAsync(x =>
					x.ProductId == dto.ProductId && x.VoteFieldId == item.VoteFieldId && x.UserId == userId);
				if (up != null) {
					up.Score = item.Score;
					await _dbContext.SaveChangesAsync();
				}
				else {
					_dbContext.Set<VoteEntity>().Add(new VoteEntity {
						ProductId = dto.ProductId,
						Score = item.Score,
						VoteFieldId = item.VoteFieldId,
						UserId = userId
					});
					await _dbContext.SaveChangesAsync();
				}
			}
			catch {
				return new GenericResponse(UtilitiesStatusCodes.BadRequest);
			}

		return new GenericResponse();
	}
}