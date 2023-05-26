namespace Utilities_aspnet.Entities; 

[Table("Promotion")]
public class PromotionEntity : BaseEntity {
	public DisplayType DisplayType { get; set; }
	public string? Skills { get; set; }
	public string? Gender { get; set; }
	public string? AgeCategories { get; set; }
	public string? States { get; set; }
	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }
	public string? Users { get; set; }
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
	public GroupChatEntity? GroupChat { get; set; }
	public Guid? GroupChatId { get; set; }
}

public class CreateUpdatePromotionDto {
	public Guid? Id { get; set; }
	public DisplayType DisplayType { get; set; }
	public List<string>? Skills { get; set; }
	public List<string>? Gender { get; set; }
	public List<string>? AgeCategories { get; set; }
	public List<string>? States { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? GroupChatId { get; set; }
}

public class PromotionDetail {
	public double TotalSeen { get; set; }
	public List<StatePerUser> StatePerUsers { get; set; }
	public List<SkillPerUser> SkillPerUsers { get; set; }
	public List<AgeCatgPerUser> AgeCatgPerUsers { get; set; }
}

public class StatePerUser {
	public string State { get; set; }
	public int UserCount { get; set; }
}

public class SkillPerUser {
	public string Skill { get; set; }
	public int UserCount { get; set; }
}

public class AgeCatgPerUser {
	public string AgeCategory { get; set; }
	public int UserCount { get; set; }
}