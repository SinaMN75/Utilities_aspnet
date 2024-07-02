namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/questionnaire")]
[Authorize]
public class QuestionnaireController(IQuestionnaireRepository repository) : BaseApiController {
	[HttpPost("Questionnaire")]
	public async Task<ActionResult<GenericResponse<QuestionnaireEntity>>> CreateQuestionnaire(QuestionnaireCreateDto dto, CancellationToken ct) =>
		Result(await repository.Create(dto, ct));

	[HttpPut("Questionnaire")]
	public async Task<ActionResult<GenericResponse<QuestionnaireEntity>>> UpdateQuestionnaire(QuestionnaireUpdateDto dto, CancellationToken ct) =>
		Result(await repository.Update(dto, ct));

	[HttpPost("Questionnaire/Filter")]
	public async Task<ActionResult<GenericResponse<IQueryable<QuestionnaireEntity>>>> FilterQuestionnaire(QuestionnaireFilterDto dto) =>
		Result(await repository.Filter(dto));

	[HttpDelete("Questionnaire/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> DeleteQuestionnaire(Guid id, CancellationToken ct) =>
		Result(await repository.Delete(id, ct));
	
	[HttpPost("QuestionnaireAnswer")]
	public async Task<ActionResult<GenericResponse<QuestionnaireAnswersEntity>>> CreateQuestionnaireAnswer(QuestionnaireAnswerCreateDto dto, CancellationToken ct) =>
		Result(await repository.CreateAnswer(dto, ct));
	
	[HttpPut("QuestionnaireAnswer")]
	public async Task<ActionResult<GenericResponse<QuestionnaireAnswersEntity>>> UpdateQuestionnaireAnswer(QuestionnaireAnswerUpdateDto dto, CancellationToken ct) =>
		Result(await repository.UpdateAnswer(dto, ct));

	[HttpDelete("QuestionnaireAnswer/{id:guid}")]
	public async Task<ActionResult<GenericResponse>> DeleteQuestionnaireAnswer(Guid id, CancellationToken ct) =>
		Result(await repository.DeleteAnswer(id, ct));
	
	[HttpPut("CreateQuestionnaireHistory")]
	public async Task<ActionResult<GenericResponse<QuestionnaireAnswersEntity>>> CreateQuestionnaireHistory(QuestionnaireHistoryCreateDto dto, CancellationToken ct) =>
		Result(await repository.CreateQuestionnaireHistory(dto, ct));
}