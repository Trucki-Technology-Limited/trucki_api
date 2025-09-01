using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllDriverResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Phone { get; set; }

        public string EmailAddress { get; set; }

        public string TruckId { get; set; }
        public string UserId { get; set; }

        public string DriversLicence { get; set; }
        public string? DotNumber { get; set; }

        public string PassportFile { get; set; }
    }
    public class AdminDriverSummaryResponseModel
    {
        public int TotalDrivers { get; set; }
        public int TotalUSDrivers { get; set; }
        public int TotalNigerianDrivers { get; set; }
        public int ActiveDrivers { get; set; }
        public int InactiveDrivers { get; set; }
        public Dictionary<string, int> DriversByCountry { get; set; } = new Dictionary<string, int>();
    }
    public class DriverResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Phone { get; set; }

        public string EmailAddress { get; set; }

        public string TruckId { get; set; }
        public string UserId { get; set; }

        public string DriversLicence { get; set; }
        public string? DotNumber { get; set; }
        public AllTruckResponseModel Truck { get; set; }
        public ICollection<DriverBankAccountResponseModel> BankAccounts { get; set; }

        public string PassportFile { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public DriverOnboardingStatus OnboardingStatus { get; set; }

        public bool HasAcceptedTerms { get; set; }
    }
}
