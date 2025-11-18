namespace trucki.Models.RequestModel;

// Dispatcher registration model
public class AddDispatcherRequestBody
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Password { get; set; }
    public string Country { get; set; } = "US"; // Country selection (US, NG, etc.)
    public string? IdCard { get; set; }
    public string? ProfilePicture { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
}

// Update dispatcher DOT and MC numbers
public class UpdateDispatcherDotMcNumbersRequestModel
{
    public string DispatcherId { get; set; }
    public string? DotNumber { get; set; }
    public string? McNumber { get; set; }
}

// Initial driver creation for dispatcher
public class AddDriverForDispatcherRequestModel
{
    // Basic driver info (minimal for initial creation)
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string address { set; get; }
    public string Password { set; get; } // For driver login

    // Dispatcher-specific fields
    public string DispatcherId { get; set; }
    public decimal CommissionPercentage { get; set; } // 0-50% range
    public bool IsDispatcherManaged { get; set; } = true;

    // Country will be inherited from dispatcher
    // Documents and truck details will be handled in separate onboarding steps
}

// Driver document upload for dispatcher
public class UploadDriverDocumentsForDispatcherDto
{
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }

    // Using existing CreateDriverDocumentRequest structure
    public List<CreateDriverDocumentRequest> Documents { get; set; } = new();

    // Additional basic driver info if not provided during initial creation
    public string? ProfilePictureUrl { get; set; }
    public string? DotNumber { get; set; } // Required for US drivers
    public string? McNumber { get; set; } // Optional for US drivers
}

// Truck addition for dispatcher driver (extends existing model)
public class AddTruckForDispatcherDriverDto : DriverAddTruckRequestModel
{
    public string DispatcherId { get; set; }
    // Inherits all existing truck fields from DriverAddTruckRequestModel
}

// Complete onboarding process
public class CompleteDriverOnboardingDto
{
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }
    public bool ConfirmAllDocumentsUploaded { get; set; }
    public bool ConfirmTruckAdded { get; set; }
}

// Bidding on behalf of driver
public class CreateBidOnBehalfDto
{
    public string OrderId { get; set; }
    public string DriverId { get; set; }  // Selected driver for the bid
    public string DispatcherId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

// Document upload for dispatcher (manifest)
public class UploadManifestOnBehalfDto
{
    public string OrderId { get; set; }
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }  // Verify dispatcher manages this driver
    public List<string> ManifestUrls { get; set; }
}

// Complete delivery for dispatcher
public class CompleteDeliveryOnBehalfDto
{
    public string OrderId { get; set; }
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }  // Verify dispatcher manages this driver
    public List<string> DeliveryDocumentUrls { get; set; }
}

// Commission update model
public class UpdateCommissionRequestModel
{
    public string DriverId { get; set; }
    public string DispatcherId { get; set; }
    public decimal NewCommissionPercentage { get; set; }
}

// CreateDriverDocumentRequest is already defined elsewhere in the codebase

// Add driver to fleet manager (unified for both dispatcher and transporter)
public class AddDriverToFleetManagerRequestModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Number { get; set; }
    public string Address { get; set; }
    public string FleetManagerId { get; set; }
}