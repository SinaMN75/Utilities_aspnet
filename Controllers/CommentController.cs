﻿namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController(ICommentRepository commentRepository) : BaseApiController {
	[HttpGet("{id:guid}")]
	[OutputCache(PolicyName = "24h")]
	public async Task<ActionResult<GenericResponse<CommentEntity>>> ReadById(Guid id) => Result(await commentRepository.ReadById(id));

	[HttpGet("ReadByProductId/{id:guid}")]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<CommentEntity>?>> ReadByProductId(Guid id) => Result(commentRepository.ReadByProductId(id));
	
	[HttpGet("ReadByUserId/{id}")]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<IQueryable<CommentEntity>?>> ReadByUserId(string id) => Result(commentRepository.ReadByUserId(id));

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> Create(CommentCreateUpdateDto parameter, CancellationToken ct) =>
		Result(await commentRepository.Create(parameter, ct));

	[HttpGet]
	[OutputCache(PolicyName = "24h")]
	public ActionResult<GenericResponse<CommentEntity>> Read(CommentFilterDto dto) => Result(commentRepository.Filter(dto));

	[HttpPost("Filter")]
	public ActionResult<GenericResponse<CommentEntity>> Filter(CommentFilterDto dto) => Result(commentRepository.Filter(dto));

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