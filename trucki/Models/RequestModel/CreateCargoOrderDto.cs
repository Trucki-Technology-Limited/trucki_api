using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class CargoOrderItemDto
    {
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public bool IsFragile { get; set; }
        public string? SpecialHandlingInstructions { get; set; }
        public CargoType Type { get; set; }
        public int Quantity { get; set; }
        public List<string> ItemImages { get; set; }
    }

    public class CreateCargoOrderDto
    {
        public string CargoOwnerId { get; set; }
        public string PickupLocation { get; set; }
        public string PickupLocationLat { get; set; }
        public string PickupLocationLong { get; set; }
        public string DeliveryLocation { get; set; }
        public string DeliveryLocationLat { get; set; }
        public string DeliveryLocationLong { get; set; }
        public string CountryCode { get; set; }
        public List<CargoOrderItemDto> Items { get; set; }
        public CargoTruckType? RequiredTruckType { get; set; }
        public bool OpenForBidding { get; set; }
        public DateTime PickupDateTime { get; set; }

        // Optional contact person fields
        public string? PickupContactName { get; set; }
        public string? PickupContactPhone { get; set; }
        public string? DeliveryContactName { get; set; }
        public string? DeliveryContactPhone { get; set; }
    }

    public class SelectDriverDto
    {
        public string OrderId { get; set; }
        public string BidId { get; set; }

    }

    public class OpenOrderForBiddingDto
    {
        public string OrderId { get; set; }
    }

    public class DriverAcknowledgementDto
    {
        public string OrderId { get; set; }
        public string BidId { get; set; }
        public bool IsAcknowledged { get; set; }
    }

    public class UploadManifestDto
    {
        public string OrderId { get; set; }
        public List<string> ManifestUrl { get; set; }
        public string? FleetManagerId { get; set; }  // For fleet managers uploading on behalf of drivers
    }

    public class UpdateLocationDto
    {
        public string OrderId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime? EstimatedTimeOfArrival { get; set; }
    }
    public class CompleteDeliveryDto
    {
        public string OrderId { get; set; }
        public List<string> DeliveryDocuments { get; set; }
        public string? FleetManagerId { get; set; }  // For fleet managers completing on behalf of drivers
    }

    public class CreateBidDto
    {
        public string OrderId { get; set; }
        public string DriverId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }  // Optional notes from driver about the bid
        public string? FleetManagerId { get; set; }  // Optional: For fleet managers bidding on behalf of drivers
    }
    public class UpdateBidDto
    {
        [Required]
        public string BidId { get; set; }

        [Required]
        public string OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? Notes { get; set; }
        public string? FleetManagerId { get; set; }  // For fleet managers updating bids on behalf of drivers
    }
    public class StartOrderDto
    {
        public string OrderId { get; set; }
        public string DriverId { get; set; }
    }
    public class GetDriverOrdersQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public CargoOrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}
