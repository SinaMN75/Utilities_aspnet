namespace Utilities_aspnet.Entities;

[Table("Withdraw")]
public class WithdrawEntity : BaseEntity {
	[MaxLength(20)]
	public string? ShebaNumber { get; set; }

	public int? Amount { get; set; }
	public WithdrawState? WithdrawState { get; set; }
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
}

public class WalletWithdrawalDto {
	public required string ShebaNumber { get; set; }
	public required int Amount { get; set; }
}

public class WithdrawalFilterDto {
	public WithdrawState? WithdrawState { get; set; }
}