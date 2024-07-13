namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[ApiKey]
public class ChatController(IChatRepository repository) : BaseApiController {
	[HttpPost("CreateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChat(GroupChatCreateUpdateDto dto) =>
		Result(await repository.CreateGroupChat(dto));

	[HttpPost("FilterGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> FilterGroupChat(GroupChatFilterDto dto) => Result(await repository.FilterGroupChats(dto));

	[HttpPut("UpdateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto) =>
		Result(await repository.UpdateGroupChat(dto));

	[HttpDelete("DeleteGroupChat/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChat(Guid id) => Result(await repository.DeleteGroupChat(id));

	[HttpPost("CreateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) =>
		Result(await repository.CreateGroupChatMessage(dto));
	
	[HttpPost("FilterGroupChatMessages")]
	public ActionResult<GenericResponse<GroupChatMessageEntity?>> FilterChatMessages(FilterGroupChatMessagesDto dto) =>
		Result(repository.FilterGroupChatMessages(dto));

	[HttpPost("SeenGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> SeenGroupChatMesDeleteGroupChatsage(Guid id) => Result(await repository.SeenGroupChatMessage(id));

	[HttpGet("ReadGroupChatById/{id:guid}")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadGroupChatById(Guid id) => Result(await repository.ReadGroupChatById(id));

	[HttpPut("UpdateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatMessageEntity?>>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) =>
		Result(await repository.UpdateGroupChatMessage(dto));

	[HttpDelete("DeleteGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChatMessage(Guid id) => Result(await repository.DeleteGroupChatMessage(id));

	[HttpPost("ExitFromGroup/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> ExitFromGroup(Guid id) => Result(await repository.ExitFromGroup(id));

	[HttpPost("Mute/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Mute(Guid id) => Result(await repository.Mute(id));
}