using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class UpdateDriverOnboardingStatusRequest
    {
        [Required]
        public DriverOnboardingStatus OnboardingStatus { get; set; }

        public string? Reason { get; set; }
    }
}