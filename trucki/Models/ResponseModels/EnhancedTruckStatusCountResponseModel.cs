namespace trucki.Models.ResponseModels
{
    /// <summary>
    /// Enhanced response model for truck status and approval status counts
    /// </summary>
    public class EnhancedTruckStatusCountResponseModel
    {
        // Truck Status Counts
        public int EnRouteCount { get; set; }
        public int AvailableCount { get; set; }
        public int BusyCount { get; set; }
        public int OutOfServiceCount { get; set; }
        
        // Approval Status Counts
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int NotApprovedCount { get; set; }
        public int BlockedCount { get; set; }
        
        // Combined Statistics
        public int TotalTrucks { get; set; }
        public int ActiveTrucks { get; set; } // Available + EnRoute + Busy
        public int DriverOwnedTrucks { get; set; }
        public int TruckOwnerOwnedTrucks { get; set; }
        
        // Additional metrics
        public int TrucksOnTrip { get; set; }
        public double ApprovalRate { get; set; } // Percentage of approved trucks
        
        /// <summary>
        /// Calculate derived properties
        /// </summary>
        public void CalculateDerivedProperties()
        {
            TotalTrucks = EnRouteCount + AvailableCount + BusyCount + OutOfServiceCount;
            ActiveTrucks = AvailableCount + EnRouteCount + BusyCount;
            TrucksOnTrip = EnRouteCount + BusyCount;
            
            if (TotalTrucks > 0)
            {
                ApprovalRate = Math.Round((double)ApprovedCount / TotalTrucks * 100, 2);
            }
        }
    }
}