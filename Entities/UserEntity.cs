namespace Utilities_aspnet.Entities;

public class UserEntity {
	public string Id { get; set; }

	[MaxLength(50)]
	public string? FirstName { get; set; }

	[MaxLength(50)]
	public string? LastName { get; set; }

	[MaxLength(50)]
	public string? FullName { get; set; }

	[MaxLength(100)]
	public string? Headline { get; set; }

	[StringLength(2000)]
	public string? Bio { get; set; }

	[MaxLength(50)]
	public string? UserName { get; set; }

	[MaxLength(50)]
	public string? AppUserName { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	[MaxLength(20)]
	public string? AppPhoneNumber { get; set; }

	[MaxLength(100)]
	public string? Email { get; set; }

	[MaxLength(100)]
	public string? AppEmail { get; set; }

	[MaxLength(20)]
	public string? UseCase { get; set; }

	[MaxLength(20)]
	public string? Type { get; set; }

	[MaxLength(100)]
	public string? AccessLevel { get; set; }

	[MaxLength(100)]
	public string? Region { get; set; }

	[MaxLength(100)]
	public string? State { get; set; }

	[MaxLength(100)]
	public string? Badge { get; set; }

	[MaxLength(20)]
	public string? JobStatus { get; set; }

	public string? MutedChats { get; set; }
	public GenderType? Gender { get; set; }
	public int? Wallet { get; set; } = 0;
	public double? Point { get; set; } = 0;
	public DateTime? Birthdate { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool? IsOnline { get; set; }
	public bool? Suspend { get; set; }
	public bool? IsPrivate { get; set; } = true;
	public DateTime? ExpireUpgradeAccount { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public UserJsonDetail JsonDetail { get; set; } = new();

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public IEnumerable<FormEntity>? FormBuilders { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string VisitedProducts { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string BookmarkedProducts { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string FollowingUsers { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string FollowedUsers { get; set; } = "";

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string BlockedUsers { get; set; } = "";

	[NotMapped]
	public bool IsFollowing { get; set; }

	[NotMapped]
	public int? CountProducts { get; set; }

	[NotMapped]
	public int? CountFollowers { get; set; } = 0;

	[NotMapped]
	public int? CountFollowing { get; set; } = 0;

	[NotMapped]
	public string? Token { get; set; }
}

public class UserJsonDetail {
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? Activity { get; set; }
	public string? Color { get; set; }
	public PrivacyType? PrivacyType { get; set; }
	public string? Code { get; set; }
	public string? ShebaNumber { get; set; }
	public LegalAuthenticationType? LegalAuthenticationType { get; set; }
	public NationalityType? NationalityType { get; set; }
}

public class GetMobileVerificationCodeForLoginDto {
	public required string Mobile { get; set; }
	public bool SendSms { get; set; } = true;
	public string? Token { get; set; }
}

public class VerifyMobileForLoginDto {
	public required string Mobile { get; set; }
	public required string VerificationCode { get; set; }
}

public class RegisterDto {
	public string? UserName { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Password { get; set; }
	public bool SendSms { get; set; } = false;
	public UserJsonDetail? JsonDetail { get; set; } = new();
}

public class LoginWithPasswordDto {
	public string? Email { get; set; }
	public string? Password { get; set; }
}

public class UserCreateUpdateDto {
	public string? Id { get; set; }
	public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? Bio { get; set; }
	public string? Headline { get; set; }
	public string? AppUserName { get; set; }
	public string? AppPhoneNumber { get; set; }
	public string? AppEmail { get; set; }
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public string? Region { get; set; }
	public string? State { get; set; }
	public string? Badge { get; set; }
	public string? VisitedProducts { get; set; }
	public string? BookmarkedProducts { get; set; }
	public string? FollowedUsers { get; set; }
	public string? FollowingUsers { get; set; }
	public string? BlockedUsers { get; set; }
	public int? Wallet { get; set; }
	public double? Point { get; set; } = 0;
	public bool? Suspend { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? JobStatus { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? Activity { get; set; }
	public string? Color { get; set; }
	public string? AccessLevel { get; set; }
	public GenderType? Gender { get; set; }
	public LegalAuthenticationType? LegalAuthenticationType { get; set; }
	public NationalityType? NationalityType { get; set; }
	public PrivacyType? PrivacyType { get; set; }
	public bool? IsOnline { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public DateTime? BirthDate { get; set; }
	public DateTime? ExpireUpgradeAccount { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public string? Code { get; set; }
	public string? ShebaNumber { get; set; }
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
	public string? JobStatus { get; set; }
	public string? AppUserName { get; set; }
	public string? AppPhoneNumber { get; set; }
	public string? AppEmail { get; set; }
	public GenderType? Gender { get; set; } = null!;
	public string? UseCase { get; set; }
	public string? Type { get; set; }
	public string? Region { get; set; }
	public string? State { get; set; }
	public string? AccessLevel { get; set; }
	public string? Badge { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public bool? ShowMedia { get; set; }
	public bool? ShowCategories { get; set; }
	public bool? ShowSuspend { get; set; }
	public bool? ShowMyCustomers { get; set; }
	public bool? OrderByUserName { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
}

public class TransferFromWalletToWalletDto {
	public string FromUserId { get; set; } = null!;
	public string ToUserId { get; set; } = null!;
	public int Amount { get; set; }
}

public class AuthorizeUserDto {
	public NationalityType NationalityType { get; set; }
	public string Code { get; set; } = null!;
	public string ShebaNumber { get; set; } = null!;
}