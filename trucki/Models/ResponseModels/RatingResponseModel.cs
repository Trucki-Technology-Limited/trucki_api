namespace trucki.Models.ResponseModels
{
    public class DriverRatingResponseModel
    {
        public string Id { get; set; }
        public string CargoOrderId { get; set; }
        public string CargoOwnerId { get; set; }
        public string CargoOwnerName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime RatedAt { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
    }

    public class DriverRatingSummaryModel
    {
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = new();
    }

    public class DriverWithRatingResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public DriverRatingSummaryModel Rating { get; set; }
    }
}