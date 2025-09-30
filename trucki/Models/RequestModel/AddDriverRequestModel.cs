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
    public string? address { set; get; }
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
public class UpdateTruckPhotosRequestModel
{
    public string TruckId { get; set; }
    public string? ExternalTruckPictureUrl { get; set; }
    public string? CargoSpacePictureUrl { get; set; }
    public string? CargoMeasurements { get; set; }
}

public class UpdateDriverProfilePhotoRequestModel
{
    [Required]
    public string DriverId { get; set; }

    [Required]
    [Url]
    public string ProfilePhotoUrl { get; set; }
}

public class GetAllDriversRequestModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    // Search filters
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }

    // Sorting
    public string? SortBy { get; set; } = "name"; // name, email, country, createdat
    public bool SortDescending { get; set; } = false;
}

public class UpdateDotNumberRequestModel
{
    [Required]
    public string DriverId { get; set; }

    [Required]
    [StringLength(12, MinimumLength = 7, ErrorMessage = "DOT number must be between 7 and 12 characters")]
    [RegularExpression(@"^\d+$", ErrorMessage = "DOT number must contain only digits")]
    public string DotNumber { get; set; }

    [StringLength(12, MinimumLength = 6, ErrorMessage = "MC number must be between 6 and 12 characters")]
    [RegularExpression(@"^\d+$", ErrorMessage = "MC number must contain only digits")]
    public string? McNumber { get; set; }
}

public class UpdateTransportationNumbersRequestModel
{
    [Required]
    public string DriverId { get; set; }

    [StringLength(12, MinimumLength = 7, ErrorMessage = "DOT number must be between 7 and 12 characters")]
    [RegularExpression(@"^\d+$", ErrorMessage = "DOT number must contain only digits")]
    public string? DotNumber { get; set; }

    [StringLength(12, MinimumLength = 6, ErrorMessage = "MC number must be between 6 and 12 characters")]
    [RegularExpression(@"^\d+$", ErrorMessage = "MC number must contain only digits")]
    public string? McNumber { get; set; }
}