namespace Utilities_aspnet.Entities;

[Table("Address")]
public class AddressEntity : BaseEntity {
	[MaxLength(50)]
	public required string ReceiverFullName { get; set; }

	[MaxLength(20)]
	public required string ReceiverPhoneNumber { get; set; }

	[MaxLength(100)]
	public required string Address { get; set; }

	[MaxLength(10)]
	public required string Pelak { get; set; }

	[MaxLength(10)]
	public required string Unit { get; set; }

	[MaxLength(20)]
	public required string PostalCode { get; set; }

	public UserEntity? User { get; set; }
	public required string UserId { get; set; }
}

public class AddressCreateDto {
	public required string ReceiverFullName { get; set; }
	public required string ReceiverPhoneNumber { get; set; }
	public required string Address { get; set; }
	public required string Pelak { get; set; }
	public required string Unit { get; set; }
	public required string PostalCode { get; set; }
	public required string UserId { get; set; }
}

public class AddressUpdateDto {
	public required Guid Id { get; set; }
	public string? ReceiverFullName { get; set; }
	public string? ReceiverPhoneNumber { get; set; }
	public string? Address { get; set; }
	public string? Pelak { get; set; }
	public string? Unit { get; set; }
	public string? PostalCode { get; set; }
}

public class AddressFilterDto : BaseFilterDto {
	public string? UserId { get; set; }
}