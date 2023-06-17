namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/withdraw")]
public class WithdrawController : BaseApiController {
	private readonly IWithdrawRepository _repository;

	public WithdrawController(IWithdrawRepository repository) => _repository = repository;

	[HttpPost("WalletWithdrawal")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> WalletWithdrawal(WalletWithdrawalDto dto) => Result(await _repository.WalletWithdrawal(dto));

	[HttpPost("Filter")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public ActionResult<GenericResponse<IQueryable<WithdrawEntity>>> Filter(WithdrawalFilterDto dto) => Result(_repository.Filter(dto));
}