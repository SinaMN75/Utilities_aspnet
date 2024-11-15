namespace Utilities_aspnet.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class QuestionController(IQuestionRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<QuestionEntity>>> Create(QuestionCreateDto dto) => Result(await repository.Create(dto));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<QuestionEntity>>>> Filter(QuestionFilterDto dto) => Result(await repository.Filter(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<QuestionEntity>>> Update(QuestionUpdateDto dto) => Result(await repository.Update(dto));

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id) => Result(await repository.Delete(id));

	[HttpPost("CreateUserAnswer")]
	public async Task<ActionResult<GenericResponse<UserEntity>>> CreateUserAnswer(UserAnswerCreateDto dto) =>
		Result(await repository.CreateUserAnswer(dto));
}