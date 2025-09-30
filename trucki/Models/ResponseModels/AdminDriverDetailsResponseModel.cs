using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    /// <summary>
    /// Enhanced response model for admin to view complete driver onboarding details
    /// </summary>
    public class AdminDriverDetailsResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public string UserId { get; set; }
        public string DriversLicence { get; set; }
        public string? DotNumber { get; set; }
        public string? McNumber { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Profile Picture
        public string? ProfilePictureUrl { get; set; }
        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);

        // Onboarding Status
        public DriverOnboardingStatus OnboardingStatus { get; set; }
        public string OnboardingStatusDisplay => OnboardingStatus.ToString();

        // Terms Acceptance
        public bool HasAcceptedLatestTerms { get; set; }
        public List<TermsAcceptanceRecordDto> TermsAcceptanceHistory { get; set; } = new List<TermsAcceptanceRecordDto>();
        public DateTime? LatestTermsAcceptedAt { get; set; }

        // Document Upload Status
        public List<DriverDocumentStatusDto> DocumentStatuses { get; set; } = new List<DriverDocumentStatusDto>();
        public DocumentUploadSummary DocumentSummary { get; set; }

        // Truck Information
        public AdminDriverTruckInfo? TruckInfo { get; set; }

        // Stripe Integration
        public string? StripeConnectAccountId { get; set; }
        public StripeAccountStatus StripeAccountStatus { get; set; }
        public bool CanReceivePayouts { get; set; }

        // Bank Accounts
        public ICollection<DriverBankAccountResponseModel> BankAccounts { get; set; } = new List<DriverBankAccountResponseModel>();

        // Overall Onboarding Progress
        public OnboardingProgressSummary OnboardingProgress { get; set; }
    }

    public class DocumentUploadSummary
    {
        public int TotalRequiredDocuments { get; set; }
        public int UploadedDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int RejectedDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public bool AllRequiredDocumentsUploaded => UploadedDocuments >= TotalRequiredDocuments;
        public bool AllDocumentsApproved => ApprovedDocuments >= TotalRequiredDocuments;
        public double CompletionPercentage => TotalRequiredDocuments > 0 ? (double)UploadedDocuments / TotalRequiredDocuments * 100 : 0;
        public double ApprovalPercentage => TotalRequiredDocuments > 0 ? (double)ApprovedDocuments / TotalRequiredDocuments * 100 : 0;
    }

    public class AdminDriverTruckInfo
    {
        public string? TruckId { get; set; }
        public string? PlateNumber { get; set; }
        public string? TruckName { get; set; }
        public string TruckCapacity { get; set; }
        public string TruckType { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalStatusDisplay => ApprovalStatus.ToString();
        public TruckStatus TruckStatus { get; set; }
        public bool IsDriverOwnedTruck { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Truck Pictures
        public string? ExternalTruckPictureUrl { get; set; }
        public string? CargoSpacePictureUrl { get; set; }
        public bool HasTruckPictures => !string.IsNullOrEmpty(ExternalTruckPictureUrl) || !string.IsNullOrEmpty(CargoSpacePictureUrl);

        // Truck Documents - this was missing!
        public List<string> Documents { get; set; } = new List<string>();
        public bool HasDocuments => Documents != null && Documents.Any();
        public int DocumentCount => Documents?.Count ?? 0;

        // Truck License Information
        public string? TruckLicenseExpiryDate { get; set; }
        public string? RoadWorthinessExpiryDate { get; set; }
        public string? InsuranceExpiryDate { get; set; }

        // Truck Owner Information (if applicable)
        public string? TruckOwnerId { get; set; }
        public string? TruckOwnerName { get; set; }
    }

    public class OnboardingProgressSummary
    {
        public bool TermsAccepted { get; set; }
        public bool ProfilePictureUploaded { get; set; }
        public bool AllDocumentsUploaded { get; set; }
        public bool AllDocumentsApproved { get; set; }
        public bool TruckAdded { get; set; }
        public bool TruckApproved { get; set; }

        public List<string> CompletedSteps { get; set; } = new List<string>();
        public List<string> PendingSteps { get; set; } = new List<string>();
        public List<string> RejectedItems { get; set; } = new List<string>();

        public int TotalSteps => 6; // Terms, Profile, Docs Upload, Docs Approval, Truck Add, Truck Approval
        public int CompletedStepCount { get; set; }
        public double CompletionPercentage => (double)CompletedStepCount / TotalSteps * 100;

        public bool CanBeApproved => TermsAccepted && ProfilePictureUploaded && AllDocumentsApproved && TruckApproved;
        public string OverallStatus { get; set; }
    }
}