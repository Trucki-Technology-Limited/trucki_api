using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class Driver : BaseClass
    {
        public string Name { get; set; }

        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        [ForeignKey("TruckId")]
        public string? TruckId { get; set; }
        public Truck? Truck { get; set; }
        public string? TruckOwnerId { get; set; }
        public TruckOwner? TruckOwner { get; set; }
        public string? DriversLicence { get; set; }

        public string? PassportFile { get; set; }
        public bool IsActive { get; set; } = true;
        [ForeignKey("User")]
        public string? UserId { get; set; }

        public User? User { get; set; }

        // New property to capture which country this driver belongs to (if relevant)
        public string Country { get; set; } = "NG";  // or "NG" or any default

        // (Optional) If you plan to use the same DocumentTypes approach:
        // you can also store the EntityType if each driver is "Driver" for sure:
        // public string EntityType { get; set; } = "Driver";
        // Stripe Connect Integration
        public string? StripeConnectAccountId { get; set; }
        public StripeAccountStatus StripeAccountStatus { get; set; } = StripeAccountStatus.NotCreated;
        public DateTime? StripeAccountCreatedAt { get; set; }
        public bool CanReceivePayouts { get; set; } = false;

        // Navigation property for documents
        public ICollection<DriverDocument> DriverDocuments { get; set; } = new List<DriverDocument>();
        public ICollection<DriverBankAccount> BankAccounts { get; set; } = new List<DriverBankAccount>();
        public ICollection<TermsAcceptanceRecord> TermsAcceptanceRecords { get; set; } = new List<TermsAcceptanceRecord>();
                public ICollection<DriverPayout> Payouts { get; set; } = new List<DriverPayout>();
        [NotMapped] // Not stored in DB, calculated
        public bool HasAcceptedLatestTerms => TermsAcceptanceRecords.Any(t => t.TermsVersion == "2025"); // Update with your current version
        public DriverOnboardingStatus OnboardingStatus { get; set; } = DriverOnboardingStatus.OboardingPending;
    }

    public enum DriverOnboardingStatus
    {
        OboardingPending,
        OnboardingInReview,
        OnboardingCompleted
    }
    public enum StripeAccountStatus
    {
        NotCreated,
        Pending,
        Restricted,
        Active,
        Rejected
    }
}
