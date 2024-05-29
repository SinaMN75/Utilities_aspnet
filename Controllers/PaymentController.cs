namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController(IPaymentRepository repository) : BaseApiController {
	[HttpPost("ng")]
	public async Task<ActionResult<GenericResponse<string>>> PayNg(long amount, string currency) =>
		Result(await repository.PayNGenius(amount, currency));
}