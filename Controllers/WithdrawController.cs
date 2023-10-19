namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/withdraw")]
public class WithdrawController(IWithdrawRepository repository) : BaseApiController {
	[HttpPost("WalletWithdrawal")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> WalletWithdrawal(WalletWithdrawalDto dto) => Result(await repository.WalletWithdrawal(dto));

	[HttpPost("Filter")]
	[Authorize]
	public ActionResult<GenericResponse<IQueryable<WithdrawEntity>>> Filter(WithdrawalFilterDto dto) => Result(repository.Filter(dto));
}