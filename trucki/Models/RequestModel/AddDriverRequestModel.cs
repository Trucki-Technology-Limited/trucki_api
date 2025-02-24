using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel;

public class AddDriverRequestModel
{
    public string Picture { set; get; }
    public string IdCard { set; get; }
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string Country { set; get; }
    public string address { set; get; }
    public string? TruckId { set; get; }
    public string? TruckOwnerId { get; set; }  // Add this property
}

public class EditDriverRequestModel 
{
    public string Id { get; set; }
    public string Name { set; get; }
    public string Number { get; set; }
    public String? ProfilePicture { get; set; }
}

public class CreateDriverRequestModel
{
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string Country { set; get; }
    public string address { set; get; }
    public string password { set; get; }
}

public class AddDriverBankAccountDto
{
    [Required]
    public string BankName { get; set; }
    [Required]
    public string DriversId { get; set; }
    
    [Required]
    public string AccountHolderName { get; set; }
    
    [Required]
    public string AccountNumber { get; set; }
    
    [Required]
    public string RoutingNumber { get; set; }
    
    public string? SwiftCode { get; set; }
}
public class DriverBankAccountResponseDto
{
    public string Id { get; set; }
    public string BankName { get; set; }
    public string AccountHolderName { get; set; }
    public string MaskedAccountNumber { get; set; }  // Only show last 4 digits
    public string RoutingNumber { get; set; }
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}