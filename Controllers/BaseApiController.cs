namespace Utilities_aspnet.Controllers;

[ApiController]
[ApiKey]
public abstract class BaseApiController : ControllerBase {
	[NonAction]
	protected static ObjectResult Result(GenericResponse response) => new(response) {
		StatusCode = response.Status.Value()
	};
}