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
public class ReactionFilterDto {
	public Guid? ProductId { get; set; }
	public Reaction? Reaction { get; set; }
    public int PageSize { get; set; } = 100;
    public int PageNumber { get; set; } = 1;
}