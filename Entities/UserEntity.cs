namespace Utilities_aspnet.Entities;

public class UserEntity : IdentityUser {
	public bool Suspend { get; set; }

	[StringLength(500)]
	public string? FirstName { get; set; }

	[StringLength(500)]
	public string? LastName { get; set; }

	[StringLength(500)]
	public string? FullName { get; set; }

	[StringLength(500)]
	public string? Headline { get; set; }

	[StringLength(2000)]
	public string? Bio { get; set; }

	[StringLength(500)]
	public string? AppUserName { get; set; }

	[StringLength(500)]
	public string? AppPhoneNumber { get; set; }

	[StringLength(500)]
	public string? AppEmail { get; set; }

	[StringLength(500)]
	public string? Instagram { get; set; }

	[StringLength(500)]
	public string? Telegram { get; set; }

	[StringLength(500)]
	public string? WhatsApp { get; set; }

	[StringLength(500)]
	public string? LinkedIn { get; set; }

	[StringLength(500)]
	public string? Dribble { get; set; }

	[StringLength(500)]
	public string? SoundCloud { get; set; }

	[StringLength(500)]
	public string? Pinterest { get; set; }

	[StringLength(500)]
	public string? Website { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[StringLength(500)]
	public string? Type { get; set; }

	[StringLength(500)]
	public string? Region { get; set; }

	[StringLength(500)]
	public string? Activity { get; set; }

	[StringLength(500)]
	public string? Color { get; set; }

	[StringLength(500)]
	public string? State { get; set; }

	[StringLength(500)]
	public string? StateTr1 { get; set; }

	[StringLength(500)]
	public string? StateTr2 { get; set; }

	[StringLength(500)]
	public string? Gender { get; set; }

	[StringLength(500)]
	public string? GenderTr1 { get; set; }

	[StringLength(500)]
	public string? GenderTr2 { get; set; }

	[StringLength(500)]
	public string? Detail1 { get; set; }

	[StringLength(500)]
	public string? Detail2 { get; set; }

	public double? Wallet { get; set; } = 0;
	public double? Point { get; set; } = 0;
	public bool? ShowContactInfo { get; set; }
	public DateTime? Birthdate { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	public string? AccessLevel { get; set; }
	public string? Badge { get; set; }
	public bool IsOnline { get; set; } = false;
	public string? MutedChats { get; set; }
	public DateTime? ExpireUpgradeAccount { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public IEnumerable<FormEntity>? FormBuilders { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public IEnumerable<ProductEntity>? Products { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public IEnumerable<TransactionEntity>? Transactions { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public IEnumerable<LikeCommentEntity>? LikeComments { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public IEnumerable<GroupChatEntity>? GroupChats { get; set; }

	public IEnumerable<AddressEntity>? Addresses { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public string VisitedProducts { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	public string BookmarkedProducts { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	public string FollowingUsers { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	public string FollowedUsers { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	public string BlockedUsers { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	public string BoughtProduts { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	public bool IsLoggedIn { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public DateTime? DeletedAt { get; set; }

	public bool IsPrivate { get; set; }

	[NotMapped]
	public bool IsFollowing { get; set; } = false;

	[NotMapped]
	public int? CountProducts { get; set; }

	[NotMapped]
	public int? CountFollowers { get; set; } = 0;

	[NotMapped]
	public int? CountFollowing { get; set; } = 0;

	[NotMapped]
	public bool IsAdmin { get; set; }

	[NotMapped]
	public string? Token { get; set; }

	[NotMapped]
	public GrowthRateReadDto? GrowthRate { get; set; }
}

[Table("Otps")]
public class OtpEntity : BaseEntity {
	public string OtpCode { get; set; }

	public UserEntity User { get; set; }
	public string UserId { get; set; }
}

public class GetMobileVerificationCodeForLoginDto {
	public string Mobile { get; set; }
	public bool SendSMS { get; set; }
	public string? token { get; set; }
}

public class VerifyMobileForLoginDto {
	public string Mobile { get; set; }
	public string VerificationCode { get; set; }
}

public class RegisterDto {
	public string? UserName { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? AccessLevel { get; set; }
	public string? Password { get; set; }
	public bool? SendSMS { get; set; }
}

public class LoginWithPasswordDto {
	public string? Email { get; set; }
	public string? Password { get; set; }
}

public class GrowthRateReadDto {
	public string? Id { get; set; }
	public double InterActive1 { get; set; }
	public double InterActive2 { get; set; }
	public double InterActive3 { get; set; }
	public double InterActive4 { get; set; }
	public double InterActive5 { get; set; }
	public double InterActive6 { get; set; }
	public double InterActive7 { get; set; }
	public double Feedback1 { get; set; }
	public double Feedback2 { get; set; }
	public double Feedback3 { get; set; }
	public double Feedback4 { get; set; }
	public double Feedback5 { get; set; }
	public double Feedback6 { get; set; }
	public double Feedback7 { get; set; }
	public double TotalInterActive { get; set; }
	public double TotalFeedback { get; set; }
}

public class UserCreateUpdateDto {
	public string? Id { get; set; }
	public string? PhoneNumber { get; set; }
	public string? UserName { get; set; }
	public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? Bio { get; set; }
	public string? Headline { get; set; }
	public string? Website { get; set; }
	public string? AppUserName { get; set; }
	public string? AppPhoneNumber { get; set; }
	public string? AppEmail { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Gender { get; set; } = null!;
	public string? GenderTr1 { get; set; }
	public string? GenderTr2 { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public string? Region { get; set; }
	public string? Activity { get; set; }
	public string? Color { get; set; }
	public string? State { get; set; }
	public string? StateTr1 { get; set; }
	public string? StateTr2 { get; set; }
	public string? AccessLevel { get; set; }
	public string? Badge { get; set; }
	public string? VisitedProducts { get; set; }
	public string? BookmarkedProducts { get; set; }
	public string? BoughtProduts { get; set; }
	public string? FollowedUsers { get; set; }
	public string? FollowingUsers { get; set; }
	public string? BlockedUsers { get; set; }
	public string? Detail1 { get; set; }
	public string? Detail2 { get; set; }
	public double? Wallet { get; set; }
	public double? Point { get; set; } = 0;
	public bool? Suspend { get; set; }
	public bool? ShowContactInfo { get; set; }
	public bool? IsOnline { get; set; }
	public bool? IsLoggedIn { get; set; }
	public bool? IsPrivate { get; set; }
	public DateTime? BirthDate { get; set; }
	public DateTime? ExpireUpgradeAccount { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
}

public class UserFilterDto {
	public string? UserId { get; set; }
	public string? UserName { get; set; }

	public string? UserNameExact { get; set; }
	public string? Query { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? Bio { get; set; }
	public string? Headline { get; set; }
	public string? Website { get; set; }
	public string? AppUserName { get; set; }
	public string? AppPhoneNumber { get; set; }
	public string? AppEmail { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Gender { get; set; } = null!;
	public string? GenderTr1 { get; set; }
	public string? GenderTr2 { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public string? Region { get; set; }
	public string? Activity { get; set; }
	public string? Color { get; set; }
	public string? State { get; set; }
	public string? StateTr1 { get; set; }
	public string? StateTr2 { get; set; }
	public string? AccessLevel { get; set; }
	public string? Badge { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;

	public bool? ShowMedia { get; set; }
	public bool? ShowCategories { get; set; }
	public bool? ShowSuspend { get; set; }
	public bool? ShowBlockedUser { get; set; }
	public bool? OrderByUserName { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
}

public class TransferFromWalletToWalletDto {
	public string FromUserId { get; set; } = null!;
	public string ToUserId { get; set; } = null!;
	public double Amount { get; set; }
}