namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TransactionController : BaseApiController {
	private readonly ITransactionRepository _repository;

	public TransactionController(ITransactionRepository repository) => _repository = repository;

	[HttpPost]
	public async Task<ActionResult<GenericResponse<TransactionReadDto>>> Create(TransactionEntity dto) => Result(await _repository.Create(dto));

	[HttpGet]
	public ActionResult<GenericResponse<IQueryable<TransactionReadDto>>> Read() => Result(_repository.Read());

	[HttpGet("Mine")]
	public ActionResult<GenericResponse<IQueryable<TransactionReadDto>>> ReadMine() => Result(_repository.ReadMine());
}