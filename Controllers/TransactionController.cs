namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiKey]
public class TransactionController(ITransactionRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<IQueryable<TransactionEntity>>>> Create(TransactionCreateDto dto, CancellationToken ct)
		=> Result(await repository.Create(dto, ct));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<TransactionEntity>>> Filter(TransactionFilterDto dto) => Result(repository.Filter(dto));

	[HttpPost("GenerateReport")]
	public async Task<ActionResult<GenericResponse<List<MediaEntity>>>> GenerateReport(TransactionFilterDto dto)
		=> Result(await repository.GenerateReport(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<IQueryable<TransactionEntity>>>> Update(TransactionUpdateDto dto, CancellationToken ct)
		=> Result(await repository.Update(dto, ct));

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}