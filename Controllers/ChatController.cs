namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController : BaseApiController {
	private readonly IChatRepository _repository;

	public ChatController(IChatRepository repository) => _repository = repository;

	[HttpPost("CreateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChat(GroupChatCreateUpdateDto dto) =>
		Result(await _repository.CreateGroupChat(dto));

	[HttpPost("FilterGroupChat")]
	public ActionResult<GenericResponse<GroupChatEntity?>> FilterGroupChat(GroupChatFilterDto dto) => Result(_repository.FilterGroupChats(dto));

	[HttpPost("FilterAllGroupChat")]
	public ActionResult<GenericResponse<GroupChatEntity?>> FilterAllGroupChat(GroupChatFilterDto dto) => Result(_repository.FilterAllGroupChats(dto));

	[HttpPut("UpdateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto) =>
		Result(await _repository.UpdateGroupChat(dto));

	[HttpDelete("DeleteGroupChat/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChat(Guid id) => Result(await _repository.DeleteGroupChat(id));

	[HttpPost("CreateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) =>
		Result(await _repository.CreateGroupChatMessage(dto));

	[HttpGet("ReadMyGroupChats")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadMyGroupChats() => Result(await _repository.ReadMyGroupChats());

	[HttpGet("ReadGroupChatMessages/{id:guid}")]
	public ActionResult<GenericResponse<GroupChatMessageEntity?>> ReadGroupChatMessages(
		Guid id,
		[FromQuery] int pageSize = 100,
		[FromQuery] int pageNumber = 1) => Result(_repository.ReadGroupChatMessages(id, pageSize, pageNumber));

	[HttpPost("SeenGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> SeenGroupChatMesDeleteGroupChatsage(Guid id) => Result(await _repository.SeenGroupChatMessage(id));

	[HttpGet("ReadGroupChatById/{id:guid}")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadGroupChatById(Guid id) => Result(await _repository.ReadGroupChatById(id));

	[HttpPut("UpdateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatMessageEntity?>>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) =>
		Result(await _repository.UpdateGroupChatMessage(dto));

	[HttpDelete("DeleteGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChatMessage(Guid id) => Result(await _repository.DeleteGroupChatMessage(id));

	[HttpPost("ExitFromGroup/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> ExitFromGroup(Guid id) => Result(await _repository.ExitFromGroup(id));

	[HttpPost("Mute/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Mute(Guid id) => Result(await _repository.Mute(id));
}