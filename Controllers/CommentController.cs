namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : BaseApiController {
	private readonly ICommentRepository _repository;

	public CommentController(ICommentRepository commentRepository) => _repository = commentRepository;

	[HttpGet("{id:guid}")]
	[OutputCache(PolicyName = "24h")]
	public async Task<ActionResult<GenericResponse<CommentEntity>>> ReadById(Guid id) => Result(await _repository.ReadById(id));

	[HttpGet("ReadByProductId/{id:guid}")]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<CommentEntity>?>> ReadByProductId(Guid id) => Result(_repository.ReadByProductId(id));

	[HttpPost]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Create(CommentCreateUpdateDto parameter, CancellationToken ct) => Result(await _repository.Create(parameter, ct));
	
	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<CommentEntity>> Read(CommentFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<CommentEntity>> Filter(CommentFilterDto dto) => Result(_repository.Filter(dto));

	[HttpPost("AddReactionToComment/{commentId:guid}/{reaction}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> AddReactionToComment(Guid commentId, Reaction reaction, CancellationToken ct) =>
		Result(await _repository.AddReactionToComment(commentId, reaction, ct));

	[HttpPut]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Update(Guid id, CommentCreateUpdateDto parameter, CancellationToken ct) => Result(await _repository.Update(id, parameter, ct));

	[HttpDelete]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id, CancellationToken ct) => Result(await _repository.Delete(id, ct));
}