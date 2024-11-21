namespace Utilities_aspnet.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class QuestionController(IQuestionRepository repository) : BaseApiController {
	[HttpPost]
	public async Task<ActionResult<GenericResponse<QuestionEntity>>> Create(QuestionCreateDto dto) => Result(await repository.Create(dto));

	[HttpPost("BulkCreate")]
	public async Task<ActionResult<GenericResponse<QuestionEntity>>> BulkCreate(List<QuestionCreateDto> dto) => 
		Result(await repository.BulkCreate(dto));

	[HttpPost("Filter")]
	public async Task<ActionResult<GenericResponse<IEnumerable<QuestionEntity>>>> Filter(QuestionFilterDto dto) => Result(await repository.Filter(dto));

	[HttpPut]
	public async Task<ActionResult<GenericResponse<QuestionEntity>>> Update(QuestionUpdateDto dto) => Result(await repository.Update(dto));

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id) => Result(await repository.Delete(id));
	
	[HttpPost("UserAnswer")]
	public async Task<ActionResult<GenericResponse<QuestionEntity>>> CreateUserAnswer(UserQuestionAnswerCreateDto dto) => 
		Result(await repository.CreateUserQuestionAnswer(dto));
	
	[HttpPost("FilterUserAnswer")]
	public async Task<ActionResult<GenericResponse<IEnumerable<QuestionEntity>>>> Filter(UserQuestionAnswerFilterDto dto) =>
		Result(await repository.FilterUserQuestionAnswer(dto));


}