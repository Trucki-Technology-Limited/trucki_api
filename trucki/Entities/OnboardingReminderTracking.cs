using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    /// <summary>
    /// Tracks onboarding reminder emails sent to drivers
    /// to avoid sending duplicate reminders
    /// </summary>
    public class OnboardingReminderTracking : BaseClass
    {
        [ForeignKey("Driver")]
        public string DriverId { get; set; }
        public Driver Driver { get; set; }

        // Track which reminder has been sent
        public bool FirstReminderSent { get; set; } = false;
        public DateTime? FirstReminderSentAt { get; set; }

        public bool SecondReminderSent { get; set; } = false;
        public DateTime? SecondReminderSentAt { get; set; }

        // Track when driver started onboarding
        public DateTime OnboardingStartedAt { get; set; }
    }
}
