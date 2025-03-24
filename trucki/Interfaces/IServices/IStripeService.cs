using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;
public interface IStripeService
{
       Task<StripePaymentResponse> CreatePaymentIntent(string orderId, decimal amount, string currency = "usd");
    Task<StripePaymentResponse> ConfirmPayment(string paymentIntentId);
    Task<bool> VerifyPaymentStatus(string paymentIntentId);
}