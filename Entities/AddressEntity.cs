namespace Utilities_aspnet.Entities;

[Table("Address")]
public class AddressEntity : BaseEntity {
	[MaxLength(500)]
	public string? ReceiverFullName { get; set; }

	[MaxLength(500)]
	public string? ReceiverPhoneNumber { get; set; }

	[MaxLength(500)]
	public string? Address { get; set; }

	[MaxLength(500)]
	public string? Pelak { get; set; }

	[MaxLength(500)]
	public string? Unit { get; set; }

	[MaxLength(500)]
	public string? PostalCode { get; set; }

	public bool IsDefault { get; set; }
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
}

public class AddressCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? ReceiverFullName { get; set; }
	public string? ReceiverPhoneNumber { get; set; }
	public string? Address { get; set; }
	public string? Pelak { get; set; }
	public string? Unit { get; set; }
	public string? PostalCode { get; set; }
}