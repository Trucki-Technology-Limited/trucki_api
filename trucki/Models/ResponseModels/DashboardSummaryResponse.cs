namespace trucki.Models.ResponseModels
{
    public class DashboardSummaryResponse
    {
        public int TotalBusiness { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalTrucks { get; set; }
        public int TotalActiveTrucks { get; set; }
        
        // New fields you requested
        public int TotalManagers { get; set; }
        public int TotalFieldOfficers { get; set; }
        public int TotalTruckOwners { get; set; }
        public int TotalOrdersCompleted { get; set; }
        public int TotalCargoOrdersCompleted { get; set; }
        public int TotalNigerianDrivers { get; set; }
        public int TotalUSDrivers { get; set; }
        public int TotalCargoOwners { get; set; }

    }
}
