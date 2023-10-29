namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController(ITransactionRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<IQueryable<TransactionEntity>>>> Create(TransactionCreateDto dto, CancellationToken ct)
		=> Result(await repository.Create(dto, ct));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> Filter(TransactionFilterDto dto) => Result(repository.Filter(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<IQueryable<TransactionEntity>>>> Update(TransactionUpdateDto dto, CancellationToken ct)
		=> Result(await repository.Update(dto, ct));

	[HttpDelete("{id}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}