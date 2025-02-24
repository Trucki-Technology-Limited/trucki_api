using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using trucki.Entities;
namespace trucki.Entities;
public class DriverBankAccount : BaseClass
{
    public string DriverId { get; set; }
    [ForeignKey("DriverId")]
    public Driver Driver { get; set; }
    
    [Required]
    public string BankName { get; set; }
    
    [Required]
    public string AccountHolderName { get; set; }
    
    [Required]
    public string AccountNumber { get; set; }
    
    [Required]
    public string RoutingNumber { get; set; }  // For US bank accounts
    
    public string? SwiftCode { get; set; }     // Optional for international transfers
    
    public bool IsDefault { get; set; } = true;
    
    public bool IsVerified { get; set; } = false;
    
    public DateTime? VerifiedAt { get; set; }
    
    public string? VerifiedBy { get; set; }
}