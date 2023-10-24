namespace Utilities_aspnet.Entities;

[Table("Withdraw")]
public class WithdrawEntity : BaseEntity {
	[MaxLength(30)]
	[MinLength(10)]
	public required string ShebaNumber { get; set; }

	public required int Amount { get; set; }
	public required WithdrawState WithdrawState { get; set; }
	public UserEntity? User { get; set; }
	public required string UserId { get; set; }
	public string? AdminMessage { get; set; }
}

public class WithdrawUpdateDto {
	public required Guid Id { get; set; }
	public required WithdrawState WithdrawState { get; set; }
	public string? AdminMessage { get; set; }
}

public class WithdrawalCreateDto {
	public required string ShebaNumber { get; set; }
	public required int Amount { get; set; }
}

public class WithdrawalFilterDto {
	public WithdrawState? State { get; set; }
	public string? UserId { get; set; }
	public bool? ShowUser { get; set; }
}