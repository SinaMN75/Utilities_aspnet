namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ChatController : BaseApiController {
	private readonly IChatRepository _repository;

	public ChatController(IChatRepository repository) => _repository = repository;

	[HttpGet]
	public async Task<ActionResult<GenericResponse<IEnumerable<ChatEntity>?>>> Read() => Result(await _repository.Read());

	[HttpGet("{userId}")]
	public async Task<ActionResult<GenericResponse<IEnumerable<ChatEntity>?>>> ReadById(string userId) => Result(await _repository.ReadByUserId(userId));

	[HttpPost]
	public async Task<ActionResult<GenericResponse<ChatEntity?>>> Create(ChatCreateUpdateDto model) => Result(await _repository.Create(model));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<ChatEntity?>>> Filter(ChatFilterDto dto) => Result(await _repository.FilterByUserId(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<ChatEntity?>>> Update(ChatCreateUpdateDto model) => Result(await _repository.Update(model));

	[HttpDelete]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await _repository.Delete(id));

	[HttpPost("CreateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChat(GroupChatCreateUpdateDto dto)
		=> Result(await _repository.CreateGroupChat(dto));

	[HttpPost("FilterGroupChat")]
	public ActionResult<GenericResponse<GroupChatEntity?>> FilterGroupChat(GroupChatFilterDto dto) => Result(_repository.FilterGroupChats(dto));

	[HttpPut("UpdateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto)
		=> Result(await _repository.UpdateGroupChat(dto));

	[HttpDelete("DeleteGroupChat/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChat(Guid id) => Result(await _repository.DeleteGroupChat(id));

	[HttpPost("CreateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
		=> Result(await _repository.CreateGroupChatMessage(dto));

	[HttpGet("ReadMyGroupChats")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadMyGroupChats() => Result(await _repository.ReadMyGroupChats());

	[HttpGet("ReadGroupChatMessages/{id:guid}")]
	public ActionResult<GenericResponse<GroupChatMessageEntity?>> ReadGroupChatMessages(Guid id, [FromQuery] int pageSize = 100, [FromQuery] int pageNumber = 1)
		=> Result(_repository.ReadGroupChatMessages(id, pageSize, pageNumber));

	[HttpPost("SeenGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> SeenGroupChatMesDeleteGroupChatsage(Guid id) => Result(await _repository.SeenGroupChatMessage(id));

	[HttpGet("ReadGroupChatById/{id:guid}")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadGroupChatById(Guid id) => Result(await _repository.ReadGroupChatById(id));

	[HttpPost("AddReactionToMessage/{emoji}/{messageId:guid}")]
	public async Task<ActionResult<GenericResponse>> AddReactionToMessage(Reaction emoji, Guid messageId)
		=> Result(await _repository.AddReactionToMessage(emoji, messageId));

	[HttpPut("UpdateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatMessageEntity?>>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
		=> Result(await _repository.UpdateGroupChatMessage(dto));

	[HttpDelete("DeleteGroupChatMessage/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChatMessage(Guid id) => Result(await _repository.DeleteGroupChatMessage(id));

	[HttpPost("ExitFromGroup/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> ExitFromGroup(Guid id) => Result(await _repository.ExitFromGroup(id));

	[HttpPost("Mute/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Mute(Guid id) => Result(await _repository.Mute(id));
}