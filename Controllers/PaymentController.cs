namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
[ApiKey]
public class PaymentController(IPaymentRepository repository) : BaseApiController {
	[HttpPost("payNG")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> PayNg(NgPayDto dto) =>
		Result(await repository.PayNg(dto));
	
	[HttpPost("payZibal")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> PayZibal(NgPayDto dto) =>
		Result(await repository.PayZibal(dto));

	[HttpPost("verifyNG/{outlet}/{id}")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> VerifyNg(string outlet, string id) =>
		Result(await repository.VerifyNg(outlet, id));
}