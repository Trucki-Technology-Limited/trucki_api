namespace trucki.Models.RequestModel;
public class PaymentIntentResponse
{
    public string PaymentIntentId { get; set; }
    public string ClientSecret { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}