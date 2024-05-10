namespace trucki.Models.ResponseModels
{
    public class GtvDashboardSummary
    {
        public decimal TotalGtv { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TruckDahsBoardData
    {
        public int CompletedOrders { get; set; }
        public int FlaggedOrders { get; set; }
        public decimal TotalOrderPrice { get; set; }
    }

    public class ManagerDashboardData
    {
        public int CompletedOrders { get; set; }
        public int FlaggedOrders { get; set; }
        public decimal TotalOrderPrice { get; set; }
    }

    public class DriverDashboardData
    {
        public int CompletedOrders { get; set; }
        public int FlaggedOrders { get; set; }
        public decimal TotalOrderPrice { get; set; }
    }
}
