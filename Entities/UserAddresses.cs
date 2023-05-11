namespace Utilities_aspnet.Entities;

[Table("Address")]
public class AddressEntity : BaseEntity
{
    public string? RecieverFullName { get; set; }
    public string? RecieverPhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Pelak { get; set; }
    public string? Unit { get; set; } //Vahed
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
    //ForeignKeys
    public UserEntity? User { get; set; }
    public string? UserId { get; set; }
}

public class AddressCreateUpdateDto
{
    public Guid? Id { get; set; }
    public string? RecieverFullName { get; set; }
    public string? RecieverPhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Pelak { get; set; }
    public string? Unit { get; set; } //Vahed
    public string? PostalCode { get; set; }
}
