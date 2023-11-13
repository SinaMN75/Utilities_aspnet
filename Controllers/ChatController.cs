namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController(IChatRepository repository) : BaseApiController {
	[HttpPost("CreateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChat(GroupChatCreateUpdateDto dto) =>
		Result(await repository.CreateGroupChat(dto));

	[HttpPost("FilterGroupChat")]
	public ActionResult<GenericResponse<GroupChatEntity?>> FilterGroupChat(GroupChatFilterDto dto) => Result(repository.FilterGroupChats(dto));

	[HttpPost("FilterAllGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> FilterAllGroupChat(GroupChatFilterDto dto) => Result(await repository.FilterAllGroupChats(dto));

	[HttpPut("UpdateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto) =>
		Result(await repository.UpdateGroupChat(dto));

	[HttpDelete("DeleteGroupChat/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChat(Guid id) => Result(await repository.DeleteGroupChat(id));

	[HttpPost("CreateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) =>
		Result(await repository.CreateGroupChatMessage(dto));

	[HttpGet("ReadMyGroupChats")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadMyGroupChats() => Result(await repository.ReadMyGroupChats());

	[HttpGet("ReadGroupChatMessages/{id}")]
	public ActionResult<GenericResponse<GroupChatMessageEntity?>> ReadGroupChatMessages(
		Guid id,
		[FromQuery] int pageSize = 100,
		[FromQuery] int pageNumber = 1) => Result(repository.ReadGroupChatMessages(id, pageSize, pageNumber));

	[HttpPost("SeenGroupChatMessage/{id}")]
	public async Task<ActionResult<GenericResponse>> SeenGroupChatMesDeleteGroupChatsage(Guid id) => Result(await repository.SeenGroupChatMessage(id));

	[HttpGet("ReadGroupChatById/{id}")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadGroupChatById(Guid id) => Result(await repository.ReadGroupChatById(id));

	[HttpPut("UpdateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatMessageEntity?>>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) =>
		Result(await repository.UpdateGroupChatMessage(dto));

	[HttpDelete("DeleteGroupChatMessage/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChatMessage(Guid id) => Result(await repository.DeleteGroupChatMessage(id));

	[HttpPost("ExitFromGroup/{id}")]
	public async Task<ActionResult<GenericResponse>> ExitFromGroup(Guid id) => Result(await repository.ExitFromGroup(id));

	[HttpPost("Mute/{id}")]
	public async Task<ActionResult<GenericResponse>> Mute(Guid id) => Result(await repository.Mute(id));
}