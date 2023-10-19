namespace Utilities_aspnet.Repositories;

public interface IFormRepository {
	GenericResponse<IQueryable<FormFieldEntity>> ReadFormFields(Guid categoryId);
	Task<GenericResponse> CreateFormField(FormFieldEntity dto);
	Task<GenericResponse<IQueryable<FormEntity>>> CreateForm(FormCreateDto model);
	Task<GenericResponse> UpdateFormField(FormFieldEntity dto);
	Task<GenericResponse> DeleteFormField(Guid id);
	Task<GenericResponse> DeleteForm(Guid id);
}

public class FormRepository(DbContext dbContext) : IFormRepository {
	public async Task<GenericResponse<IQueryable<FormEntity>>> CreateForm(FormCreateDto model) {
		foreach (FormTitleDto item in model.Form!)
			try {
				FormEntity? up = await dbContext.Set<FormEntity>()
					.FirstOrDefaultAsync(x => ((x.ProductId == model.ProductId &&
					                            model.ProductId != null) || (x.UserId == model.UserId && model.UserId != null)) &&
					                          x.FormFieldId == item.Id);
				if (up != null) {
					up.Title = item.Title ?? "";
					await dbContext.SaveChangesAsync();
				}
				else {
					dbContext.Set<FormEntity>().Add(new FormEntity {
						ProductId = model.ProductId,
						UserId = model.UserId,
						FormFieldId = item.Id,
						Title = item.Title ?? ""
					});
				}

				await dbContext.SaveChangesAsync();
			}
			catch { }

		IQueryable<FormEntity> entity = dbContext.Set<FormEntity>()
			.Where(x => (x.ProductId == model.ProductId && model.ProductId != null) || (x.UserId == model.UserId && model.UserId != null))
			.AsNoTracking();
		return new GenericResponse<IQueryable<FormEntity>>(entity);
	}

	public async Task<GenericResponse> CreateFormField(FormFieldEntity dto) {
		await dbContext.Set<FormFieldEntity>().AddAsync(dto);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> UpdateFormField(FormFieldEntity dto) {
		FormFieldEntity entity = (await dbContext.Set<FormFieldEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id))!;
		entity.Label = dto.Label;
		entity.OptionList = dto.OptionList;
		entity.CategoryId = dto.CategoryId;
		entity.IsRequired = dto.IsRequired;
		entity.Type = dto.Type;
		entity.UpdatedAt = DateTime.Now;
		dbContext.Update(entity);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<FormFieldEntity>> ReadFormFields(Guid categoryId) {
		return new GenericResponse<IQueryable<FormFieldEntity>>(dbContext.Set<FormFieldEntity>()
			.Where(x => x.CategoryId == categoryId)
			.AsNoTracking());
	}

	public async Task<GenericResponse> DeleteFormField(Guid id) {
		await dbContext.Set<FormFieldEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> DeleteForm(Guid id) {
		await dbContext.Set<FormEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}
}