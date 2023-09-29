namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : BaseApiController {
	private readonly ITransactionRepository _repository;

	public TransactionController(ITransactionRepository repository) => _repository = repository;

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> Filter(TransactionFilterDto dto) => Result(_repository.Filter(dto));
}