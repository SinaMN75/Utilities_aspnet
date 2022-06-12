using Utilities_aspnet.Chat;

namespace Utilities_aspnet.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ChatController : BaseApiController {
    private readonly IChatRepository _chatRepository;

    public ChatController(IChatRepository chatRepository) {
        _chatRepository = chatRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IEnumerable<ChatReadDto>?>>> Read() {
        GenericResponse<IEnumerable<ChatReadDto>?> i = await _chatRepository.Read();
        return Result(i);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<GenericResponse<IEnumerable<ChatReadDto>?>>> ReadById(string userId) {
        GenericResponse<IEnumerable<ChatReadDto>?> i = await _chatRepository.ReadByUserId(userId);
        return Result(i);
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<ChatReadDto?>>> Create(ChatCreateUpdateDto model) {
        GenericResponse<ChatReadDto?> i = await _chatRepository.Create(model);
        return Result(i);
    }

    // [HttpPost]
    // public async Task<ActionResult<GenericResponse<ChatGroupReadDto?>>> CreateGroup(ChatGroupCreateDto model)
    // {
    //     GenericResponse<ChatGroupReadDto?> i = await _chatRepository.CreateGroup(model);
    //     return Result(i);
    // }
    //
    // [HttpPost]
    // public async Task<ActionResult<GenericResponse<ChatGroupReadDto?>>> AddMemberToGroup(AddMemberToGroup model)
    // {
    //     GenericResponse<ChatGroupReadDto?> i = await _chatRepository.AddMemebrToGroup(model);
    //     return Result(i);
    // }

}