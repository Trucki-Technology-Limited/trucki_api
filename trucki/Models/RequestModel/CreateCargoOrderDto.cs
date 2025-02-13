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
        public string SpecialHandlingInstructions { get; set; }
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
    }

    public class SelectDriverDto
    {
        public string OrderId { get; set; }
        public string BidId { get; set; }
        public DateTime PickupDateTime { get; set; }
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
    }

    public class CreateBidDto
    {
        public string OrderId { get; set; }
        public string DriverId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }  // Optional notes from driver about the bid
    }
      public class StartOrderDto
    {
        public string OrderId { get; set; }
        public string DriverId { get; set; }
    }
}
