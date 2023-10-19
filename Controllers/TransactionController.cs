namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController(ITransactionRepository repository) : BaseApiController {
	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> Filter(TransactionFilterDto dto) => Result(repository.Filter(dto));
}