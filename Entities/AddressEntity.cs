namespace Utilities_aspnet.Entities;

[Table("Address")]
public class AddressEntity : BaseEntity {
	[MaxLength(50)]
	public string ReceiverFullName { get; set; } = null!;

	[MaxLength(20)]
	public string ReceiverPhoneNumber { get; set; } = null!;

	[MaxLength(100)]
	public string Address { get; set; } = null!;

	[MaxLength(10)]
	public string Pelak { get; set; } = null!;

	[MaxLength(10)]
	public string Unit { get; set; } = null!;

	[MaxLength(20)]
	public string PostalCode { get; set; } = null!;

	public bool IsDefault { get; set; }
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
}

public class AddressCreateDto {
	public required string ReceiverFullName { get; set; }
	public required string ReceiverPhoneNumber { get; set; }
	public required string Address { get; set; }
	public required string Pelak { get; set; }
	public required string Unit { get; set; }
	public required string PostalCode { get; set; }
	public required bool IsDefault { get; set; } = false;
}

public class AddressUpdateDto {
	public required Guid Id { get; set; }
	public string? ReceiverFullName { get; set; }
	public string? ReceiverPhoneNumber { get; set; }
	public string? Address { get; set; }
	public string? Pelak { get; set; }
	public string? Unit { get; set; }
	public string? PostalCode { get; set; }
	public bool IsDefault { get; set; } = false;
}

public class AddressFilterDto : BaseFilterDto {
	public string? UserId { get; set; }
}