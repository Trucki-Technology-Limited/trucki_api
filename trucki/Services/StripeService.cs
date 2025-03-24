using Stripe;
using trucki.DatabaseContext;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;
public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly TruckiDBContext _dbContext;

    public StripeService(IConfiguration configuration, TruckiDBContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        // Initialize Stripe with your secret key
        Stripe.StripeConfiguration.ApiKey = _configuration.GetSection("Stripe").GetSection("SecretKey").Value;
    }

    public async Task<StripePaymentResponse> CreatePaymentIntent(string orderId, decimal amount, string currency = "usd")
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert to cents
            Currency = currency,
            Metadata = new Dictionary<string, string>
            {
                { "OrderId", orderId }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return new StripePaymentResponse
        {
            PaymentIntentId = intent.Id,
            ClientSecret = intent.ClientSecret,
            Amount = amount,
            OrderId = orderId,
            Currency = currency,
            Status = intent.Status
        };
    }

    public async Task<StripePaymentResponse> ConfirmPayment(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId);

        return new StripePaymentResponse
        {
            PaymentIntentId = intent.Id,
            ClientSecret = intent.ClientSecret,
            Amount = (decimal)intent.Amount / 100,
            Currency = intent.Currency,
            Status = intent.Status
        };
    }

    public async Task<bool> VerifyPaymentStatus(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId);

        return intent.Status == "succeeded";
    }
}