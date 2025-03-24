using System.ComponentModel.DataAnnotations;

namespace trucki.Models.ResponseModels;

public class StripePaymentResponse
{
    public string PaymentIntentId { get; set; }
    public string ClientSecret { get; set; }
    public decimal Amount { get; set; } // This is the total amount
    public string Currency { get; set; }
    public string Status { get; set; }
    public string OrderId { get; set; }
    public PaymentBreakdown PaymentBreakdown { get; set; }
}

public class PaymentBreakdown
{
    public decimal BidAmount { get; set; }
    public decimal SystemFee { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
}
public class CreatePaymentIntentDto
{
    [Required]
    public string OrderId { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}

public class ConfirmPaymentDto
{
    [Required]
    public string OrderId { get; set; }
    [Required]
    public string PaymentIntentId { get; set; }
}