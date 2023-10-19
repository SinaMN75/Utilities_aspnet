namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormController(IFormRepository repository) : BaseApiController {
	[HttpPost("CreateFormField")]
	public async Task<ActionResult<GenericResponse<IQueryable<FormFieldEntity>>>> CreateFormField(FormFieldEntity dto) =>
		Result(await repository.CreateFormField(dto));

	[HttpPut("UpdateFormField")]
	public async Task<ActionResult<GenericResponse<IQueryable<FormFieldEntity>>>> UpdateFormField(FormFieldEntity dto) =>
		Result(await repository.UpdateFormField(dto));

	[AllowAnonymous]
	[HttpGet("{categoryId:guid}")]
	public ActionResult<GenericResponse<IQueryable<FormFieldEntity>>> ReadFormFieldById(Guid categoryId) => Result(repository.ReadFormFields(categoryId));

	[HttpPost]
	public async Task<ActionResult<GenericResponse<IQueryable<FormEntity>>>> CreateForm(FormCreateDto model) => Result(await repository.CreateForm(model));

	[HttpDelete("DeleteFormField/{id:guid}")]
	public async Task<IActionResult> DeleteFormField(Guid id) => Ok(await repository.DeleteFormField(id));

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteFormBuilder(Guid id) => Ok(await repository.DeleteForm(id));
}