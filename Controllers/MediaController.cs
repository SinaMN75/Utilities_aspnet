namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : BaseApiController {
	private readonly IMediaRepository _repository;

	public MediaController(IMediaRepository repository) => _repository = repository;

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Upload([FromForm] UploadDto dto) => Result(await _repository.Upload(dto));

	[HttpDelete("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await _repository.Delete(id));

	[HttpPut("{id:guid}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<MediaEntity>>> Update(Guid id, UpdateMediaDto updateMediaDto) =>
		Result(await _repository.UpdateMedia(id, updateMediaDto));
}