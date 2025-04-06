namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController(IPaymentRepository repository, AvreenIpgService service) : BaseApiController {
	[HttpPost("payNG")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> PayNg(NgPayDto dto) =>
		Result(await repository.PayNg(dto));

	[HttpPost("payZibal")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> PayZibal(NgPayDto dto) =>
		Result(await repository.PayZibal(dto));

	[HttpPost("verifyNG/{outlet}/{id}")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> VerifyNg(string outlet, string id) =>
		Result(await repository.VerifyNg(outlet, id));

	[HttpPost("verifyZibal/{outlet}/{id}")]
	public async Task<ActionResult<GenericResponse<NgHostedResponse>>> VerifyZibal(string outlet, string id) =>
		Result(await repository.VerifyZibal(outlet, id));
	
	[HttpPost("GetTokenAvreen")]
	public async Task<ActionResult> GetToken([FromBody] IpgGetTokenParams body) {
		return Ok(await service.GetTokenAsync(body));
	}

	[HttpGet("PayAvreen")]
	public ActionResult Pay([FromQuery] string token) {
		return Redirect(service.GetPaymentUrl(token));
	}

	[HttpPost("ConfirmAvreen")]
	public async Task<ActionResult> Confirm([FromForm] IpgRedirectResult data) {
		return Ok(await service.ConfirmPaymentAsync(data));
	}
	
}