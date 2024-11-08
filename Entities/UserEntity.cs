namespace Utilities_aspnet.Entities;

public class UserEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public string Id { get; set; } = null!;

	[MaxLength(50)]
	public string? FirstName { get; set; }

	[MaxLength(50)]
	public string? LastName { get; set; }

	[MaxLength(50)]
	public string? FullName { get; set; }
	
	[MaxLength(50)]
	public string? Title { get; set; }
	
	[MaxLength(50)]
	public string? Subtitle { get; set; }

	[MaxLength(100)]
	public string? Headline { get; set; }

	[MaxLength(2000)]
	public string? Bio { get; set; }

	[MaxLength(50)]
	public string? AppUserName { get; set; }

	[MaxLength(20)]
	public string? AppPhoneNumber { get; set; }

	[MaxLength(50)]
	public string? UserName { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	[MaxLength(50)]
	public string? AppEmail { get; set; }

	[MaxLength(50)]
	public string? Email { get; set; }

	[MaxLength(100)]
	public string? Country { get; set; }
	
	[MaxLength(100)]
	public string? State { get; set; }
	
	[MaxLength(100)]
	public string? City { get; set; }

	public GenderType? Gender { get; set; }
	public DateTime? Birthdate { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public DateTime? PremiumExpireDate { get; set; }
	public bool? Suspend { get; set; }
	public UserJsonDetail JsonDetail { get; set; } = new();

	public List<TagUser> Tags { get; set; } = [];

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }

	[MaxLength(50)]
	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string? Password { get; set; }

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
	public string? Color { get; set; }
	public string? Code { get; set; }
	public string? ShebaNumber { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public string? NationalCode { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? PostalCode { get; set; }
	public string? LandlinePhone { get; set; }
	public string? SchoolName { get; set; }
	public string? Height { get; set; }
	public string? Weight { get; set; }
	public string? FoodAllergies { get; set; }
	public string? Sickness { get; set; }
	public string? UsedDrugs { get; set; }
	public string? FcmToken { get; set; }
	public string? Degree { get; set; }
	public string? Institute { get; set; }
	public string? HouseholdNumber { get; set; }
	public string? UniqueId { get; set; }
	public string? PassportNumber { get; set; }
	public string? HealthReport1 { get; set; }
	public string? HealthReport2 { get; set; }
	public long? DeliveryPrice1 { get; set; }
	public long? DeliveryPrice2 { get; set; }
	public long? DeliveryPrice3 { get; set; }
	public PrivacyType? PrivacyType { get; set; }
	public NationalityType? NationalityType { get; set; }
	public List<UserSubscriptions>? UserSubscriptions { get; set; } = [];
	public List<string>? StringList { get; set; } = [];
}

public class UserSubscriptions {
	public string? ContentId { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public int? Days { get; set; }
	public List<KeyValue> KeyValues { get; set; } = [];
	public long? Price { get; set; }
	public string? TransactionRefId { get; set; }
	public List<TagSubscription>? Tags { get; set; }
	public DateTime? ExpiresIn { get; set; }
}

public class GetMobileVerificationCodeForLoginDto {
	public required string Mobile { get; set; }
}

public class VerifyMobileForLoginDto {
	public required string Mobile { get; set; }
	public required string VerificationCode { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? UserName { get; set; }
	public string? Instagram { get; set; }
	public string? FcmToken { get; set; }
}

public class LoginWithPasswordDto {
	public string? Email { get; set; }
	public string? Password { get; set; }
}

public class UserCreateUpdateDto {
	public string? Id { get; set; }
	public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? UserName { get; set; }
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? PhoneNumber { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? Bio { get; set; }
	public string? Headline { get; set; }
	public string? AppUserName { get; set; }
	public string? AppPhoneNumber { get; set; }
	public string? AppEmail { get; set; }
	public string? State { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public string? FcmToken { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? PostalCode { get; set; }
	public string? LandlinePhone { get; set; }
	public string? WhatsApp { get; set; }
	public string? LinkedIn { get; set; }
	public string? Dribble { get; set; }
	public string? SoundCloud { get; set; }
	public string? Pinterest { get; set; }
	public string? Website { get; set; }
	public string? Color { get; set; }
	public string? Password { get; set; }
	public string? Code { get; set; }
	public string? ShebaNumber { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public string? NationalCode { get; set; }
	public string? SchoolName { get; set; }
	public string? Height { get; set; }
	public string? Weight { get; set; }
	public string? FoodAllergies { get; set; }
	public string? Sickness { get; set; }
	public string? UsedDrugs { get; set; }
	public string? Degree { get; set; }
	public string? Institute { get; set; }
	public string? HouseholdNumber { get; set; }
	public string? UniqueId { get; set; }
	public string? PassportNumber { get; set; }
	public string? HealthReport1 { get; set; }
	public string? HealthReport2 { get; set; }
	public long? DeliveryPrice1 { get; set; }
	public long? DeliveryPrice2 { get; set; }
	public long? DeliveryPrice3 { get; set; }
	public bool? Suspend { get; set; }
	public GenderType? Gender { get; set; }
	public NationalityType? NationalityType { get; set; }
	public PrivacyType? PrivacyType { get; set; }
	public DateTime? BirthDate { get; set; }
	public DateTime? PremiumExpireDate { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public List<TagUser>? Tags { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
	public List<string>? StringList { get; set; }
}

public class UserFilterDto : BaseFilterDto {
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
	public string? State { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public bool? HasAppUserName { get; set; }
	public bool ShowMedia { get; set; } = false;
	public bool? ShowPremiums { get; set; }
	public bool ShowCategories { get; set; } = false;
	public bool? ShowSuspend { get; set; }
	public bool? ShowMyCustomers { get; set; }
	public bool? OrderByUserName { get; set; }
	public bool? OrderByCreatedAt { get; set; }
	public bool? OrderByCreatedAtDesc { get; set; }
	public bool? OrderByUpdatedAt { get; set; }
	public bool? OrderByUpdatedAtDesc { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public IEnumerable<string>? PhoneNumbers { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public List<TagUser>? Tags { get; set; }
}