namespace Utilities_aspnet.Repositories;

public interface IQuestionRepository {
	Task<GenericResponse<QuestionEntity>> Create(QuestionCreateDto dto);
	GenericResponse<IQueryable<QuestionEntity>> Filter(QuestionFilterDto dto);
	Task<GenericResponse<QuestionEntity?>> Update(QuestionUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
}

public class QuestionRepository : IQuestionRepository {
	public Task<GenericResponse<QuestionEntity>> Create(QuestionCreateDto dto) {
		throw new NotImplementedException();
	}

	public GenericResponse<IQueryable<QuestionEntity>> Filter(QuestionFilterDto dto) {
		throw new NotImplementedException();
	}

	public Task<GenericResponse<QuestionEntity?>> Update(QuestionUpdateDto dto) {
		throw new NotImplementedException();
	}

	public Task<GenericResponse> Delete(Guid id) {
		throw new NotImplementedException();
	}
}