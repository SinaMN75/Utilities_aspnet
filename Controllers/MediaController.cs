namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(IMediaRepository repository) : BaseApiController {
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Upload([FromForm] UploadDto dto) => Result(await repository.Upload(dto));

	[HttpPost("Filter")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Filter(MediaFilterDto dto) => Result(await repository.Filter(dto));

	[HttpDelete("{id}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await repository.Delete(id));

	[HttpPut("{id}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Update(Guid id, UpdateMediaDto updateMediaDto) => Result(await repository.Update(id, updateMediaDto));
}