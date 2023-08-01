namespace Utilities_aspnet.Repositories;

public interface IFormRepository {
	GenericResponse<IQueryable<FormFieldEntity>> ReadFormFields(Guid categoryId);
	Task<GenericResponse> CreateFormField(FormFieldEntity dto);
	Task<GenericResponse<IQueryable<FormEntity>>> CreateForm(FormCreateDto model);
	Task<GenericResponse> UpdateFormField(FormFieldEntity dto);
	Task<GenericResponse> DeleteFormField(Guid id);
	Task<GenericResponse> DeleteForm(Guid id);
}

public class FormRepository : IFormRepository {
	private readonly DbContext _dbContext;

	public FormRepository(DbContext dbContext) => _dbContext = dbContext;

	public async Task<GenericResponse<IQueryable<FormEntity>>> CreateForm(FormCreateDto model) {
		foreach (FormTitleDto item in model.Form!)
			try {
				FormEntity? up = await _dbContext.Set<FormEntity>()
					.FirstOrDefaultAsync(x => ((x.ProductId == model.ProductId &&
					                            model.ProductId != null) || (x.UserId == model.UserId && model.UserId != null)) &&
					                          x.FormFieldId == item.Id);
				if (up != null) {
					up.Title = item.Title ?? "";
					await _dbContext.SaveChangesAsync();
				}
				else {
					_dbContext.Set<FormEntity>().Add(new FormEntity {
						ProductId = model.ProductId,
						UserId = model.UserId,
						FormFieldId = item.Id,
						Title = item.Title ?? ""
					});
				}

				await _dbContext.SaveChangesAsync();
			}
			catch { }

		IQueryable<FormEntity> entity = _dbContext.Set<FormEntity>()
			.Where(x => (x.ProductId == model.ProductId && model.ProductId != null) || (x.UserId == model.UserId && model.UserId != null))
			.AsNoTracking();
		return new GenericResponse<IQueryable<FormEntity>>(entity);
	}

	public async Task<GenericResponse> CreateFormField(FormFieldEntity dto) {
		await _dbContext.Set<FormFieldEntity>().AddAsync(dto);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> UpdateFormField(FormFieldEntity dto) {
		FormFieldEntity entity = (await _dbContext.Set<FormFieldEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id))!;
		entity.Label = dto.Label;
		entity.OptionList = dto.OptionList;
		entity.CategoryId = dto.CategoryId;
		entity.IsRequired = dto.IsRequired;
		entity.Type = dto.Type;
		entity.UpdatedAt = DateTime.Now;
		_dbContext.Update(entity);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<FormFieldEntity>> ReadFormFields(Guid categoryId) {
		return new GenericResponse<IQueryable<FormFieldEntity>>(_dbContext.Set<FormFieldEntity>()
			                                                        .Where(x => x.CategoryId == categoryId)
			                                                        .AsNoTracking());
	}

	public async Task<GenericResponse> DeleteFormField(Guid id) {
		await _dbContext.Set<FormFieldEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> DeleteForm(Guid id) {
		await _dbContext.Set<FormEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}
}