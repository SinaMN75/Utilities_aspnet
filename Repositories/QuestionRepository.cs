namespace Utilities_aspnet.Repositories;

public interface IQuestionRepository {
	Task<GenericResponse<QuestionEntity>> Create(QuestionCreateDto dto);
	Task<GenericResponse<IEnumerable<QuestionEntity>>> Filter(QuestionFilterDto dto);
	Task<GenericResponse<QuestionEntity?>> Update(QuestionUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class QuestionRepository(DbContext dbContext) : IQuestionRepository {
	public async Task<GenericResponse<QuestionEntity>> Create(QuestionCreateDto dto) {
		List<CategoryEntity> categories = [];
		foreach (Guid item in dto.Categories)
			categories.Add((await dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item))!);

		EntityEntry<QuestionEntity> i = await dbContext.AddAsync(new QuestionEntity {
			Tags = dto.Tags,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			Categories = categories,
			JsonDetail = new QuestionJsonDetail { Question = dto.Question, Answers = dto.Answers }
		});

		await dbContext.SaveChangesAsync();
		return new GenericResponse<QuestionEntity>(i.Entity);
	}

	public async Task<GenericResponse<IEnumerable<QuestionEntity>>> Filter(QuestionFilterDto dto) {
		IQueryable<QuestionEntity> q = dbContext.Set<QuestionEntity>();

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));
		if (dto.Tags is not null) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).AsNoTracking();
		return new GenericResponse<IEnumerable<QuestionEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<QuestionEntity?>> Update(QuestionUpdateDto dto) {
		QuestionEntity e = (await dbContext.Set<QuestionEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id))!;

		if (dto.Tags is not null) e.Tags = dto.Tags;
		if (dto.Question is not null) e.JsonDetail.Question = dto.Question;
		if (dto.Answers is not null) e.JsonDetail.Answers = dto.Answers;
		e.UpdatedAt = DateTime.UtcNow;
		await dbContext.SaveChangesAsync();
		return new GenericResponse<QuestionEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await dbContext.Set<QuestionEntity>().Where(x => x.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}
}