﻿namespace Utilities_aspnet.Repositories;

public interface IQuestionnaireRepository {
	Task<GenericResponse<QuestionnaireEntity>> Create(QuestionnaireCreateDto dto, CancellationToken ct);
	Task<GenericResponse<IQueryable<QuestionnaireEntity>>> Filter(QuestionnaireFilterDto dto);
	Task<GenericResponse<QuestionnaireEntity?>> Update(QuestionnaireUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
	Task<GenericResponse<QuestionnaireAnswersEntity>> CreateAnswer(QuestionnaireAnswerCreateDto dto, CancellationToken ct);
	Task<GenericResponse<QuestionnaireAnswersEntity?>> UpdateAnswer(QuestionnaireAnswerUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> DeleteAnswer(Guid id, CancellationToken ct);
	Task<GenericResponse> CreateQuestionnaireHistory(QuestionnaireHistoryCreateDto dto, CancellationToken ct);
}

public class QuestionnaireRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : IQuestionnaireRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<QuestionnaireEntity>> Create(QuestionnaireCreateDto dto, CancellationToken ct) {
		EntityEntry<QuestionnaireEntity> e = await dbContext.Set<QuestionnaireEntity>().AddAsync(new QuestionnaireEntity {
			CreatedAt = DateTime.UtcNow,
			Question = dto.Question,
			Title = dto.Title
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<QuestionnaireEntity>(e.Entity);
	}

	public async Task<GenericResponse<QuestionnaireEntity?>> Update(QuestionnaireUpdateDto dto, CancellationToken ct) {
		QuestionnaireEntity e = (await dbContext.Set<QuestionnaireEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (dto.Title is not null) e.Title = dto.Title;
		if (dto.Question is not null) e.Title = dto.Question;

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<QuestionnaireEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		await dbContext.Set<QuestionnaireEntity>().Where(f => f.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse<IQueryable<QuestionnaireEntity>>> Filter(QuestionnaireFilterDto dto) {
		IQueryable<QuestionnaireEntity> q = dbContext.Set<QuestionnaireEntity>().AsNoTracking().Select(x => new QuestionnaireEntity {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Title = x.Title,
			Question = x.Question,
			Answers = x.Answers.Select(y => new QuestionnaireAnswersEntity {
				Title = y.Title,
				QuestionnaireId = y.Id,
				Value = y.Value,
				Point = y.Point
			})
		});
		
		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);
		return new GenericResponse<IQueryable<QuestionnaireEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
	
	public async Task<GenericResponse<QuestionnaireAnswersEntity>> CreateAnswer(QuestionnaireAnswerCreateDto dto, CancellationToken ct) {
		EntityEntry<QuestionnaireAnswersEntity> e = await dbContext.Set<QuestionnaireAnswersEntity>().AddAsync(new QuestionnaireAnswersEntity {
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Title = dto.Title,
			Value = dto.Value,
			Point = dto.Point,
			QuestionnaireId = dto.QuestionnaireId
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<QuestionnaireAnswersEntity>(e.Entity);
	}
	
	public async Task<GenericResponse<QuestionnaireAnswersEntity?>> UpdateAnswer(QuestionnaireAnswerUpdateDto dto, CancellationToken ct) {
		QuestionnaireAnswersEntity e = (await dbContext.Set<QuestionnaireAnswersEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (dto.Title is not null) e.Title = dto.Title;

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<QuestionnaireAnswersEntity?>(e);
	}

	public async Task<GenericResponse> DeleteAnswer(Guid id, CancellationToken ct) {
		await dbContext.Set<QuestionnaireEntity>().Where(f => f.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse> CreateQuestionnaireHistory(QuestionnaireHistoryCreateDto dto, CancellationToken ct) {
		EntityEntry<QuestionnaireHistoryEntity> e = await dbContext.Set<QuestionnaireHistoryEntity>().AddAsync(new QuestionnaireHistoryEntity {
			UserId = dto.UserId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonDetail = new QuestionnaireHistoryJsonDetail { QuestionnaireHistoryQa = dto.QuestionnaireHistoryQa }
		}, ct);
		
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<QuestionnaireHistoryEntity>(e.Entity);
	}
}