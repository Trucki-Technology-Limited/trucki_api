namespace trucki.Entities;

  public class Bid : BaseClass
    {
        public string OrderId { get; set; }
        public CargoOrders Order { get; set; }
        public string TruckId { get; set; }
        public Truck Truck { get; set; }
        public decimal Amount { get; set; }
        public BidStatus Status { get; set; }
        public DateTime? DriverAcknowledgedAt { get; set; }
    }


    public enum BidStatus
    {
        Pending,
        AdminApproved,
        AdminRejected,
        CargoOwnerSelected,
        DriverAcknowledged,
        DriverDeclined,
        Expired
    }