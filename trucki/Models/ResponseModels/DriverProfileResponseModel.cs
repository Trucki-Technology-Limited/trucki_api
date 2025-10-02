using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class DriverProfileResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string EmailAddress { get; set; }
    public string TruckId { get; set; }
    public string DriversLicence { get; set; }
    public string? DotNumber { get; set; }
    public string? McNumber { get; set; }
    public string PassportFile { get; set; }
    public string Country { get; set; }
    public bool IsActive { get; set; }
    public string UserId { get; set; }
    public bool HasAcceptedTerms { get; set; }
    public DriverOnboardingStatus OnboardingStatus { get; set; }
    public AllTruckResponseModel? Truck { get; set; }
    public DriverRatingSummaryModel Rating { get; set; }
}