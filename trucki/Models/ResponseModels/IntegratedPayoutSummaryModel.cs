namespace trucki.Models.ResponseModels;

public class IntegratedPayoutSummaryModel
{
    public DateTime ProcessedAt { get; set; }
    public int StripePayouts { get; set; }
    public int WalletPayouts { get; set; }
    public int FailedPayouts { get; set; }
    public int SkippedDrivers { get; set; }
    public decimal TotalStripeAmount { get; set; }
    public decimal TotalWalletAmount { get; set; }
    public List<PayoutErrorModel> Errors { get; set; } = new List<PayoutErrorModel>();
}

public enum PayoutMethod
{
    None,
    StripeConnect,
    Wallet
}