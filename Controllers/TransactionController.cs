namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TransactionController : BaseApiController {
	private readonly ITransactionRepository _repository;

	public TransactionController(ITransactionRepository repository) => _repository = repository;

	[HttpPost]
	public async Task<ActionResult<GenericResponse<TransactionEntity>>> Create(TransactionEntity dto) => Result(await _repository.Create(dto));

	[HttpGet]
	[OutputCache(PolicyName = "default")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> Read() => Result(_repository.Read());

	[HttpGet("Mine")]
	[OutputCache(PolicyName = "default")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> ReadMine() => Result(_repository.ReadMine());
}