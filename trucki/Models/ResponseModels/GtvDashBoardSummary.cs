namespace trucki.Models.ResponseModels
{
    public class GtvDashboardSummary
    {
        public float TotalGtv { get; set; }
        public float TotalPayout { get; set; }
        public float TotalRevenue { get; set; }
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
}
