namespace Utilities_aspnet.Entities;

[Table("Withdraw")]
public class WithdrawEntity : BaseEntity {
	public string? ShebaNumber { get; set; }
	public double? Amount { get; set; }
	public WithdrawState? WithdrawState { get; set; }
	public UserEntity? ApplicantUserEntity { get; set; }
	public string? ApplicantUserId { get; set; }
	public UserEntity? AdminUserEntity { get; set; }
	public string? AdminUserId { get; set; }
}

public class WalletWithdrawalDto {
	public required string ShebaNumber { get; set; }
	public required double Amount { get; set; }
}

public class WithdrawalFilterDto {
	public WithdrawState? WithdrawState { get; set; }
}