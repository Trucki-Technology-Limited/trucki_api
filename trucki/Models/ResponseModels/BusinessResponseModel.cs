using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class BusinessResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
    public List<RouteResponseModel>? Routes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // New Business Metrics
    public BusinessMetrics Metrics { get; set; }
}

public class BusinessMetrics
{
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalGTV { get; set; }
    public decimal TotalPayout { get; set; }
    public decimal TotalRevenue { get; set; }
}