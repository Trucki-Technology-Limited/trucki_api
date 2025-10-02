namespace trucki.Models.ResponseModels
{
    public class PendingApprovalsResponseModel
    {
        public int PendingDrivers { get; set; }
        public int PendingDocuments { get; set; }
        public int PendingTrucks { get; set; }
        public List<PendingDriverSummary> DriversPendingReview { get; set; } = new List<PendingDriverSummary>();
        public List<PendingDocumentSummary> DocumentsPendingReview { get; set; } = new List<PendingDocumentSummary>();
        public List<PendingTruckSummary> TrucksPendingReview { get; set; } = new List<PendingTruckSummary>();
    }

    public class PendingDriverSummary
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string Email { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int PendingDocumentsCount { get; set; }
        public bool HasPendingTruck { get; set; }
        public string Country { get; set; }
    }

    public class PendingDocumentSummary
    {
        public string DocumentId { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string DocumentTypeName { get; set; }
        public string FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class PendingTruckSummary
    {
        public string TruckId { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }
        public string TruckType { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsDriverOwnedTruck { get; set; }
    }

    public class BatchApprovalResponseModel
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyProcessed { get; set; }
        public int Failed { get; set; }
        public List<string> SuccessfulIds { get; set; } = new List<string>();
        public List<BatchApprovalError> Errors { get; set; } = new List<BatchApprovalError>();
        public List<string> AutoApprovedDrivers { get; set; } = new List<string>();
    }

    public class BatchApprovalError
    {
        public string ItemId { get; set; }
        public string Error { get; set; }
    }
}