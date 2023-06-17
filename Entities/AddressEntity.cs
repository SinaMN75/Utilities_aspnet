namespace Utilities_aspnet.Entities;

[Table("Address")]
public class AddressEntity : BaseEntity {
	public string? ReceiverFullName { get; set; }
	public string? ReceiverPhoneNumber { get; set; }
	public string? Address { get; set; }
	public string? Pelak { get; set; }
	public string? Unit { get; set; }
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
	public bool? IsDefault { get; set; }
}

public class AddressFilterDto
{
	public string? UserId { get; set; }
    public bool? OrderByReceiverFullName { get; set; }
    public bool? OrderByReceiverPhoneNumber { get; set; }
    public bool? OrderByAddress { get; set; }
    public bool? OrderByPelak { get; set; }
    public bool? OrderByUnit { get; set; }
    public bool? OrderByPostalCode { get; set; }
    public bool? OrderByIsDefault { get; set; }
    public int PageSize { get; set; } = 100;
    public int PageNumber { get; set; } = 1;

}