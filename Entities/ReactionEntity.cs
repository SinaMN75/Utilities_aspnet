namespace Utilities_aspnet.Entities;

[Table("Reaction")]
public class ReactionEntity : BaseEntity {
	public Reaction? Reaction { get; set; }
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
	public Guid? ChatsId { get; set; }
	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }
}

public class ReactionCreateUpdateDto {
	public Reaction? Reaction { get; set; }
	public Guid? ProductId { get; set; }
}

public class ReactionFilterDto : BaseFilterDto {
	public Guid? ProductId { get; set; }
	public Reaction? Reaction { get; set; }
}