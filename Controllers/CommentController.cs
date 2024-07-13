namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class CommentController(ICommentRepository commentRepository) : BaseApiController {
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenericResponse<CommentEntity>>> ReadById(Guid id) => Result(await commentRepository.ReadById(id));

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Create(CommentCreateUpdateDto parameter, CancellationToken ct) =>
		Result(await commentRepository.Create(parameter, ct));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<CommentEntity>>> Filter(CommentFilterDto dto) => Result(await commentRepository.Filter(dto));

	[HttpPost("AddReactionToComment/{commentId:guid}/{reaction}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> AddReactionToComment(Guid commentId, Reaction reaction, CancellationToken ct) =>
		Result(await commentRepository.AddReactionToComment(commentId, reaction, ct));

	[HttpPut]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Update(Guid id, CommentCreateUpdateDto parameter, CancellationToken ct) =>
		Result(await commentRepository.Update(id, parameter, ct));

	[HttpDelete]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await commentRepository.Delete(id, ct));
}