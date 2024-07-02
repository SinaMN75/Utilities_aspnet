namespace Utilities_aspnet.Entities;

[Table("Questionnaires")]
public class QuestionnaireEntity : BaseEntity {
	public required string Title { get; set; }
	public required string Question { get; set; }

	public IEnumerable<QuestionnaireAnswersEntity> Answers { get; set; } = null!;
}

[Table("QuestionnaireAnswers")]
public class QuestionnaireAnswersEntity : BaseEntity {
	public required string Title { get; set; }

	public QuestionnaireEntity Questionnaire { get; set; } = null!;
	public required Guid QuestionnaireId { get; set; }
}

[Table("QuestionnaireHistory")]
public class QuestionnaireHistoryEntity : BaseEntity {
	public UserEntity User { get; set; } = null!;
	public required string UserId { get; set; }
	
	public QuestionnaireHistoryJsonDetail JsonDetail { get; set; } = new();
}

public class QuestionnaireHistoryJsonDetail {
	public List<QuestionnaireHistoryQa> QuestionnaireHistoryQa { get; set; } = [];
}

public class QuestionnaireHistoryQa {
	public required string Question { get; set; }
	public required string Answer { get; set; }
}

public class QuestionnaireCreateDto {
	public required string Title { get; set; }
	public required string Question { get; set; }
}

public class QuestionnaireUpdateDto {
	public required Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Question { get; set; }
}

public class QuestionnaireAnswerCreateDto {
	public required string Title { get; set; }
	public required Guid QuestionnaireId { get; set; }
}

public class QuestionnaireAnswerUpdateDto {
	public required Guid Id { get; set; }
	public string? Title { get; set; }
}

public class QuestionnaireFilterDto : BaseFilterDto;