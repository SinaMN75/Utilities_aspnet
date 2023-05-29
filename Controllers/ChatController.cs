namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ChatController : BaseApiController {
	private readonly IChatRepository _repository;

	public ChatController(IChatRepository repository) { _repository = repository; }

	[HttpGet]
	public async Task<ActionResult<GenericResponse<IEnumerable<ChatEntity>?>>> Read() { return Result(await _repository.Read()); }

	[HttpGet("{userId}")]
	public async Task<ActionResult<GenericResponse<IEnumerable<ChatEntity>?>>> ReadById(string userId) {
		return Result(await _repository.ReadByUserId(userId));
	}

	[HttpPost]
	public async Task<ActionResult<GenericResponse<ChatEntity?>>> Create(ChatCreateUpdateDto model) { return Result(await _repository.Create(model)); }

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<ChatEntity?>>> Filter(ChatFilterDto dto) { return Result(await _repository.FilterByUserId(dto)); }

	[HttpPut]
	public async Task<ActionResult<GenericResponse<ChatEntity?>>> Update(ChatCreateUpdateDto model) { return Result(await _repository.Update(model)); }

	[HttpDelete]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) { return Result(await _repository.Delete(id)); }

	[HttpPost("CreateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChat(GroupChatCreateUpdateDto dto) {
		return Result(await _repository.CreateGroupChat(dto));
	}

	[HttpPost("FilterGroupChat")]
	public ActionResult<GenericResponse<GroupChatEntity?>> FilterGroupChat(GroupChatFilterDto dto) { return Result(_repository.FilterGroupChats(dto)); }

	[HttpPut("UpdateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto) {
		return Result(await _repository.UpdateGroupChat(dto));
	}

	[HttpDelete("DeleteGroupChat/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChat(Guid id) { return Result(await _repository.DeleteGroupChat(id)); }

	[HttpPost("CreateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) {
		return Result(await _repository.CreateGroupChatMessage(dto));
	}

	[HttpGet("ReadMyGroupChats")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadMyGroupChats() { return Result(await _repository.ReadMyGroupChats()); }

	[HttpGet("ReadGroupChatMessages/{id:guid}")]
	public ActionResult<GenericResponse<GroupChatMessageEntity?>> ReadGroupChatMessages(
		Guid id,
		[FromQuery] int pageSize = 100,
		[FromQuery] int pageNumber = 1) {
		return Result(_repository.ReadGroupChatMessages(id, pageSize, pageNumber));
	}

	[HttpPost("SeenGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> SeenGroupChatMesDeleteGroupChatsage(Guid id) { return Result(await _repository.SeenGroupChatMessage(id)); }

	[HttpGet("ReadGroupChatById/{id:guid}")]
	public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> ReadGroupChatById(Guid id) { return Result(await _repository.ReadGroupChatById(id)); }

	[HttpPost("AddReactionToMessage/{emoji}/{messageId:guid}")]
	public async Task<ActionResult<GenericResponse>> AddReactionToMessage(Reaction emoji, Guid messageId) {
		return Result(await _repository.AddReactionToMessage(emoji, messageId));
	}

	[HttpPut("UpdateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatMessageEntity?>>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) {
		return Result(await _repository.UpdateGroupChatMessage(dto));
	}

	[HttpDelete("DeleteGroupChatMessage/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChatMessage(Guid id) { return Result(await _repository.DeleteGroupChatMessage(id)); }

	[HttpPost("ExitFromGroup/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> ExitFromGroup(Guid id) { return Result(await _repository.ExitFromGroup(id)); }

	[HttpPost("Mute/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> Mute(Guid id) { return Result(await _repository.Mute(id)); }
}