namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Content2Controller(IContentRepository2 repository) : BaseApiController {
	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<ContentEntity>>> Read() => Result(repository.Read());

	// [HttpPost]
	// [Authorize]
	// public async Task<ActionResult<GenericResponse<ContentEntity>>> Create(ContentCreateUpdateDto dto, CancellationToken ct) =>
	// 	Result(await repository.Create(dto, ct));
	//
	// [HttpPut]
	// [Authorize]
	// public async Task<ActionResult<GenericResponse<ContentEntity>>> Update(ContentCreateUpdateDto dto, CancellationToken ct) =>
	// 	Result(await repository.Update(dto, ct));
	//
	// [HttpDelete("{id:guid}")]
	// [Authorize]
	// public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Result(await repository.Delete(id, ct));
}