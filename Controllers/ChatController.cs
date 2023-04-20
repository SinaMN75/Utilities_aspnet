namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ChatController : BaseApiController {
	private readonly IChatRepository _repository;

	public ChatController(IChatRepository repository) => _repository = repository;

	[HttpGet]
	public async Task<ActionResult<GenericResponse<IEnumerable<ChatReadDto>?>>> Read() => Result(await _repository.Read());

	[HttpGet("{userId}")]
	public async Task<ActionResult<GenericResponse<IEnumerable<ChatReadDto>?>>> ReadById(string userId) => Result(await _repository.ReadByUserId(userId));

	[HttpPost]
	public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Create(ChatCreateUpdateDto model) => Result(await _repository.Create(model));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Filter(ChatFilterDto dto) => Result(await _repository.FilterByUserId(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Update(ChatCreateUpdateDto model) => Result(await _repository.Update(model));

	[HttpDelete]
	public async Task<ActionResult<GenericResponse>> Delete(Guid id) => Result(await _repository.Delete(id));

	[HttpPost("CreateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatReadDto?>>> CreateGroupChat(GroupChatCreateUpdateDto dto)
		=> Result(await _repository.CreateGroupChat(dto));

	[HttpPost("FilterGroupChat")]
	public ActionResult<GenericResponse<GroupChatReadDto?>> FilterGroupChat(GroupChatFilterDto dto) => Result(_repository.FilterGroupChats(dto));

	[HttpPut("UpdateGroupChat")]
	public async Task<ActionResult<GenericResponse<GroupChatReadDto?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto)
		=> Result(await _repository.UpdateGroupChat(dto));

	[HttpDelete("DeleteGroupChat/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChat(Guid id) => Result(await _repository.DeleteGroupChat(id));

	[HttpPost("CreateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatReadDto?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
		=> Result(await _repository.CreateGroupChatMessage(dto));

	[HttpGet("ReadMyGroupChats")]
	public async Task<ActionResult<GenericResponse<GroupChatReadDto?>>> ReadMyGroupChats() => Result(await _repository.ReadMyGroupChats());

	[HttpGet("ReadGroupChatMessages/{id:guid}")]
	public ActionResult<GenericResponse<GroupChatMessageReadDto?>> ReadGroupChatMessages(Guid id) => Result(_repository.ReadGroupChatMessages(id));

	[HttpPost("SeenGroupChatMessage/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> SeenGroupChatMesDeleteGroupChatsage(Guid id) => Result(await _repository.SeenGroupChatMessage(id));

	[HttpGet("ReadGroupChatById/{id:guid}")]
	public async Task<ActionResult<GenericResponse<GroupChatReadDto?>>> ReadGroupChatById(Guid id) => Result(await _repository.ReadGroupChatById(id));

	[HttpPost("AddReactionToMessage/{emoji}/{messageId:guid}")]
	public async Task<ActionResult<GenericResponse>> AddReactionToMessage(Reaction emoji, Guid messageId)
		=> Result(await _repository.AddReactionToMessage(emoji, messageId));

	[HttpPut("UpdateGroupChatMessage")]
	public async Task<ActionResult<GenericResponse<GroupChatMessageReadDto?>>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
		=> Result(await _repository.UpdateGroupChatMessage(dto));

	[HttpDelete("DeleteGroupChatMessage/{id}")]
	public async Task<ActionResult<GenericResponse>> DeleteGroupChatMessage(Guid id) => Result(await _repository.DeleteGroupChatMessage(id));

	[HttpPost("ExitFromGroup/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> ExitFromGroup(Guid id) => Result(await _repository.ExitFromGroup(id));
}