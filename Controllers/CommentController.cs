namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : BaseApiController {
	private readonly ICommentRepository _repository;

	public CommentController(ICommentRepository commentRepository) => _repository = commentRepository;

	[HttpGet("{id:guid}")]
	[OutputCache(PolicyName = "10s")]
	public async Task<ActionResult<GenericResponse<CommentEntity>>> Read(Guid id) => Result(await _repository.Read(id));

	[HttpGet("ReadByProductId/{id:guid}")]
	[OutputCache(PolicyName = "10s")]
	public ActionResult<GenericResponse<IQueryable<CommentEntity>?>> ReadByProductId(Guid id) => Result(_repository.ReadByProductId(id));

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Create(CommentCreateUpdateDto parameter) => Result(await _repository.Create(parameter));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<CommentEntity>> Filter(CommentFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPost("AddReactionToComment/{commentId:guid}/{reaction}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> AddReactionToComment(Guid commentId, Reaction reaction)
		=> Result(await _repository.AddReactionToComment(commentId, reaction));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Update(Guid id, CommentCreateUpdateDto parameter) => Result(await _repository.Update(id, parameter));

	[HttpDelete]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await _repository.Delete(id));
}