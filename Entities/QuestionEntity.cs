namespace Utilities_aspnet.Entities;

[Table("Questions")]
public class QuestionEntity : BaseEntity {
	public required QuestionJsonDetail JsonDetail { get; set; }
	public required List<TagQuestion> Tags { get; set; }
}

public class QuestionJsonDetail {
	public required string Question { get; set; }
	public required List<AnswerDetail> Answers { get; set; }
}

public class AnswerDetail {
	public required string Answer { get; set; }
	public required string Hint { get; set; }
}

public class QuestionCreateDto {
	public required string Question { get; set; }
	public required List<AnswerDetail> Answers { get; set; }
	public required List<TagQuestion> Tags { get; set; }
}

public class QuestionUpdateDto {
	public required Guid Id { get; set; }
	public string? Question { get; set; }
	public List<AnswerDetail>? Answers { get; set; }
	public List<TagQuestion>? Tags { get; set; }
}

public class QuestionFilterDto : BaseFilterDto {
	public List<TagQuestion>? Tags { get; set; }
}
