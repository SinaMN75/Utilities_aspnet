namespace Utilities_aspnet.Repositories;

public interface IQuestionRepository {
	Task<GenericResponse<QuestionEntity>> Create(QuestionCreateDto dto);
	Task<GenericResponse> BulkCreate(List<QuestionCreateDto> dto);
	Task<GenericResponse<IEnumerable<QuestionEntity>>> Filter(QuestionFilterDto dto);
	Task<GenericResponse<QuestionEntity?>> Update(QuestionUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
	
	Task<GenericResponse<UserQuestionAnswerEntity>> CreateUserQuestionAnswer(UserQuestionAnswerCreateDto dto);
	Task<GenericResponse<IEnumerable<UserQuestionAnswerEntity>>> FilterUserQuestionAnswer(UserQuestionAnswerFilterDto dto);
}

public class QuestionRepository(DbContext dbContext) : IQuestionRepository {
	public async Task<GenericResponse<QuestionEntity>> Create(QuestionCreateDto dto) {
		EntityEntry<QuestionEntity> i = await dbContext.AddAsync(new QuestionEntity {
			Tags = dto.Tags,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			CategoryId = dto.CategoryId,
			JsonDetail = new QuestionJsonDetail { Question = dto.Question, Answers = dto.Answers }
		});

		await dbContext.SaveChangesAsync();
		return new GenericResponse<QuestionEntity>(i.Entity);
	}

	public async Task<GenericResponse> BulkCreate(List<QuestionCreateDto> dto) {
		List<QuestionEntity> list = [];
		
		foreach (QuestionCreateDto i in dto) {
			list.Add(new QuestionEntity {
				Tags = i.Tags,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				CategoryId = i.CategoryId,
				JsonDetail = new QuestionJsonDetail { Question = i.Question, Answers = i.Answers }
			});
		}

		await dbContext.AddRangeAsync(list);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<IEnumerable<QuestionEntity>>> Filter(QuestionFilterDto dto) {
		IQueryable<QuestionEntity> q = dbContext.Set<QuestionEntity>().Include(x => x.Category);

		if (dto.CategoryId is not null) q = q.Where(x => x.CategoryId == dto.CategoryId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

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

	public async Task<GenericResponse<UserQuestionAnswerEntity>> CreateUserQuestionAnswer(UserQuestionAnswerCreateDto dto) {
		EntityEntry<UserQuestionAnswerEntity> e = await dbContext.AddAsync(new UserQuestionAnswerEntity {
			UserId = dto.UserId,
			JsonDetail = new UserQuestionAnswerJsonDetail { UserQuestionAnswer = dto.UserQuestionAnswer }
		});
		
		await dbContext.SaveChangesAsync();

		return new GenericResponse<UserQuestionAnswerEntity>(e.Entity);
	}

	public async Task<GenericResponse<IEnumerable<UserQuestionAnswerEntity>>> FilterUserQuestionAnswer(UserQuestionAnswerFilterDto dto) {
		IQueryable<UserQuestionAnswerEntity> q = dbContext.Set<UserQuestionAnswerEntity>();

		if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.UserId));

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).AsNoTracking();
		return new GenericResponse<IEnumerable<UserQuestionAnswerEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}