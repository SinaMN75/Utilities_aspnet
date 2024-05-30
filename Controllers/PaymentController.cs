namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController(IPaymentRepository repository) : BaseApiController {
	[HttpPost("ng")]
	public async Task<ActionResult<GenericResponse<NgeniusHostedResponse>>> PayNg(NgeniusPaymentDto dto) =>
		Result(await repository.PayNGenius(dto));
}