namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/withdraw")]
[Authorize]
public class WithdrawController(IWithdrawRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse>> Create(WithdrawalCreateDto createDto) => Result(await repository.Create(createDto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<IQueryable<WithdrawEntity>>> Filter(WithdrawalFilterDto dto) => Result(repository.Filter(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<WithdrawEntity?>>> Update(WithdrawUpdateDto dto) => Result(await repository.Update(dto));
}