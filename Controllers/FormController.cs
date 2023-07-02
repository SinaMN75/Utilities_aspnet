namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormController : BaseApiController {
	private readonly IFormRepository _repository;

	public FormController(IFormRepository repository) => _repository = repository;

	[HttpPost("CreateFormField")]
	public async Task<ActionResult<GenericResponse<IQueryable<FormFieldEntity>>>> CreateFormField(FormFieldEntity dto) =>
		Result(await _repository.CreateFormField(dto));

	[HttpPut("UpdateFormField")]
	public async Task<ActionResult<GenericResponse<IQueryable<FormFieldEntity>>>> UpdateFormField(FormFieldEntity dto) =>
		Result(await _repository.UpdateFormField(dto));

	[AllowAnonymous]
	[HttpGet("{categoryId:guid}")]
	[OutputCache(PolicyName = "default")]
	public ActionResult<GenericResponse<IQueryable<FormFieldEntity>>> ReadFormFieldById(Guid categoryId) => Result(_repository.ReadFormFields(categoryId));

	[HttpPost]
	public async Task<ActionResult<GenericResponse<IQueryable<FormEntity>>>> CreateForm(FormCreateDto model) => Result(await _repository.CreateForm(model));

	[HttpDelete("DeleteFormField/{id:guid}")]
	public async Task<IActionResult> DeleteFormField(Guid id) => Ok(await _repository.DeleteFormField(id));

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteFormBuilder(Guid id) => Ok(await _repository.DeleteForm(id));
}