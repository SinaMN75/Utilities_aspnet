using Pushe.co;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ChatController : BaseApiController
{
    private readonly IChatRepository _repository;

    public ChatController(IChatRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IEnumerable<ChatReadDto>?>>> Read() => Result(await _repository.Read());

    [HttpGet("{userId}")]
    public async Task<ActionResult<GenericResponse<IEnumerable<ChatReadDto>?>>> ReadById(string userId) => Result(await _repository.ReadByUserId(userId));

    [HttpPost]
    public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Create([FromServices] IPusheService _pusheService, ChatCreateUpdateDto model)
    {
        var result = await _repository.Create(model);
        var push = new PusheJsonModel
        {
            Applications = new string[] { "YooHoo" },
            Notification = new Notification
            {
                Title = "Message Created",
                Content = JsonConvert.SerializeObject(result.Result)
            }
        };
        await _pusheService.SendAsync(push);
        return Result(result);
    }

    [HttpPost("Filter")]
    public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Filter(ChatFilterDto dto) => Result(await _repository.FilterByUserId(dto));

    [HttpPut]
    public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Update(ChatCreateUpdateDto model) => Result(await _repository.Update(model));

    [HttpDelete]
    public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Delete(Guid id) => Result(await _repository.Delete(id));

    [HttpPost("CreateGroupChat")]
    public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChat(GroupChatCreateUpdateDto dto)
        => Result(await _repository.CreateGroupChat(dto));

    [HttpPost("FilterGroupChat")]
    public ActionResult<GenericResponse<GroupChatEntity?>> FilterGroupChat(GroupChatFilterDto dto) => Result(_repository.FilterGroupChats(dto));

    [HttpPut("UpdateGroupChat")]
    public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> UpdateGroupChat(GroupChatCreateUpdateDto dto)
        => Result(await _repository.UpdateGroupChat(dto));

    [HttpPost("CreateGroupChatMessage")]
    public async Task<ActionResult<GenericResponse<GroupChatEntity?>>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
        => Result(await _repository.CreateGroupChatMessage(dto));

    [HttpGet("ReadMyGroupChats")]
    public ActionResult<GenericResponse<GroupChatMessageEntity?>> ReadMyGroupChats() => Result(_repository.ReadMyGroupChats());

    [HttpGet("ReadGroupChatMessages/{id:guid}")]
    public ActionResult<GenericResponse<GroupChatMessageEntity?>> ReadGroupChatMessages(Guid id) => Result(_repository.ReadGroupChatMessages(id));

    [HttpGet("ReadGroupChatById/{id:guid}")]
    public async Task<ActionResult<GenericResponse<GroupChatMessageEntity?>>> ReadGroupChatById(Guid id) => Result(await _repository.ReadGroupChatById(id));

    [HttpPost("AddReactionToMessage/{emoji}/{messageId:guid}")]
    public async Task<ActionResult<GenericResponse>> AddReactionToMessage(Reaction emoji, Guid messageId) => Result(await _repository.AddReactionToMessage(emoji, messageId));
}