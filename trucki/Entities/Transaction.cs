namespace trucki.Entities;

public class Transaction : BaseClass
{
    public string OrderId { get; set; }
    public Order Order { get; set; } // Navigation property to the Order

    public string? TruckId { get; set; }
    public Truck? Truck { get; set; } // Optional, if truck info is relevant

    public string BusinessId { get; set; }
    public Business Business { get; set; }

    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionType Type { get; set; }
}

public enum TransactionType
{
    InitialPayment, // Could be used for any initial payments
    SixtyPercentPayment,
    FortyPercentPayment,
    Other // For future flexibility
}