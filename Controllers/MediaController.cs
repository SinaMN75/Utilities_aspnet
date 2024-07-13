namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class MediaController(IMediaRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	[RequestSizeLimit(512 * 1024 * 1024)]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Upload([FromForm] UploadDto dto) => Result(await repository.Upload(dto));

	[HttpPost("Filter")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Filter(MediaFilterDto dto) => Result(await repository.Filter(dto));

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await repository.Delete(id));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Update(UpdateMediaDto updateMediaDto) => Result(await repository.Update(updateMediaDto));
}