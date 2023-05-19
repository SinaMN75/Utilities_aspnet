using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/withdraw")]
public class WithdrawController
{
    private readonly IWithdrawRepository _withdrawRepository;
    public WithdrawController(IWithdrawRepository withdrawRepository) => _withdrawRepository = withdrawRepository;

    [HttpPost("WalletWithdrawal")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<GenericResponse>> WalletWithdrawal(WalletWithdrawalDto dto) => Result(await _repository.WalletWithdrawal(dto));
}
