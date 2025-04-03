namespace trucki.Models.ResponseModels;

public class DriverBankAccountResponseModel
{
 public string Id { get; set; }
    public string DriverId { get; set; }



    public string BankName { get; set; }


    public string AccountHolderName { get; set; }


    public string AccountNumber { get; set; }


    public string RoutingNumber { get; set; }  // For US bank accounts

    public string? SwiftCode { get; set; }     // Optional for international transfers

    public bool IsDefault { get; set; } = true;

    public bool IsVerified { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }

    public string? VerifiedBy { get; set; }
}