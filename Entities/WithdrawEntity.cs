namespace Utilities_aspnet.Entities;

[Table("Withdraw")]
public class WithdrawEntity : BaseEntity {
	[MaxLength(20)]
	public required string? ShebaNumber { get; set; }

	public required int Amount { get; set; }
	public required WithdrawState WithdrawState { get; set; }
	public UserEntity? User { get; set; }
	public required string UserId { get; set; }
	public string? AdminMessage { get; set; }
}

public class WithdrawCreateUpdateDto {
	public required Guid Id { get; set; }
	public required WithdrawState WithdrawState { get; set; }
	public string? AdminMessage { get; set; }
}

public class WalletWithdrawalDto {
	public required string ShebaNumber { get; set; }
	public required int Amount { get; set; }
}

public class WithdrawalFilterDto {
	public WithdrawState? State { get; set; }
	public string? UserId { get; set; }
	public bool? ShowUser { get; set; }
}