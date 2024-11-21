namespace Utilities_aspnet.Entities;

[Table("Questions")]
public class QuestionEntity : BaseEntity {
	public required QuestionJsonDetail JsonDetail { get; set; }
	public required List<TagQuestion> Tags { get; set; }

	public required IEnumerable<CategoryEntity> Categories { get; set; }
}

[Table("Questions")]
public class UserQuestionAnswerEntity : BaseEntity {
	public required string UserId { get; set; }
	public UserEntity? User { get; set; }
	public required UserQuestionAnswerJsonDetail JsonDetail { get; set; } = new();
}

public class QuestionJsonDetail {
	public required string Question { get; set; }
	public required List<AnswerDetail> Answers { get; set; }
}

public class UserQuestionAnswerJsonDetail {
	public List<UserQuestionAnswerJson> UserQuestionAnswer { get; set; } = [];
}

public class AnswerDetail {
	public required string Answer { get; set; }
	public required string Hint { get; set; }
	public required int Point { get; set; }
}

public class UserQuestionAnswerJson {
	public required string Question { get; set; }
	public required string Answer { get; set; }
	public required string Hint { get; set; }
	public required int Point { get; set; }
}

public class QuestionCreateDto {
	public required string Question { get; set; }
	public required List<AnswerDetail> Answers { get; set; }
	public required List<TagQuestion> Tags { get; set; }
	public required IEnumerable<Guid> Categories { get; set; }
}

public class UserQuestionAnswerCreateDto {
	public required string UserId { get; set; }
	public required List<UserQuestionAnswerJson> UserQuestionAnswer { get; set; } 
}

public class QuestionUpdateDto {
	public required Guid Id { get; set; }
	public string? Question { get; set; }
	public List<AnswerDetail>? Answers { get; set; }
	public List<TagQuestion>? Tags { get; set; }
}

public class QuestionFilterDto : BaseFilterDto {
	public IEnumerable<Guid>? Categories { get; set; }
	public List<TagQuestion>? Tags { get; set; }
}

public class UserQuestionAnswerFilterDto : BaseFilterDto {
	public List<string>? UserIds { get; set; }
}
