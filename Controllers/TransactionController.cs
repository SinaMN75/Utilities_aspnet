namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : BaseApiController {
	private readonly ITransactionRepository _repository;

	public TransactionController(ITransactionRepository repository) => _repository = repository;

	[HttpPost]
	public async Task<ActionResult<GenericResponse<TransactionEntity>>> Create(TransactionEntity dto, CancellationToken ct) => Result(await _repository.Create(dto, ct));

	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> Read(TransactionFilterDto dto) => Result(_repository.Filter(dto));

	[HttpGet("Mine")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> ReadMine() => Result(_repository.ReadMine());
}