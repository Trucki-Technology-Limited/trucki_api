using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class AssignDriverToTruckRequestModel
    {
        public string TruckId { get; set; }
        public string DriverId { get; set; }
    }
    
    public class UpdateTruckStatusRequestModel
    {
        public TruckStatus TruckStatus { get; set; }
    }
}
