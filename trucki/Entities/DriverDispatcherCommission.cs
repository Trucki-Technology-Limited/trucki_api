namespace trucki.Entities;

public class DriverDispatcherCommission : BaseClass
{
    public string DriverId { get; set; }
    public Driver Driver { get; set; }
    public string DispatcherId { get; set; } // TruckOwner ID
    public TruckOwner Dispatcher { get; set; }
    public decimal CommissionPercentage { get; set; } // Dispatcher's cut (e.g., 15%)
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}