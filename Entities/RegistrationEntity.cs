namespace Utilities_aspnet.Entities;

[Table("Registration")]
public class RegistrationEntity : BaseEntity {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public int? Row { get; set; }
	public int? Column { get; set; }

	public UserEntity User { get; set; } = null!;
	public required string UserId { get; set; }
	
	public ProductEntity? Product { get; set; }
	public required Guid ProductId { get; set; }
}

public class RegistrationCreateDto {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public int? Row { get; set; }
	public int? Column { get; set; }

	public required string UserId { get; set; }
	
	public required Guid ProductId { get; set; }
}

public class RegistrationUpdateDto {
	public required Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public int? Row { get; set; }
	public int? Column { get; set; }
}

public class RegistrationFilterDto : BaseFilterDto {
	public string? UserId { get; set; }
	public Guid? ProductId { get; set; }
}