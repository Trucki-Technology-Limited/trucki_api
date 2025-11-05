namespace trucki.Interfaces.IServices;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string otp);
    Task SendOrderEmailAsync(string toEmail, string subject, string date, string orderId, string status, string businessName, string deliveryAddress);
    Task SendPasswordResetEmailAsync(string toEmail, string subject, string resetCode);
    Task SendWelcomeEmailAsync(string toEmail, string name, string userType, string confirmationLink);
    Task SendPaymentReceiptEmailAsync(string toEmail, string orderId, decimal bidAmount, decimal systemFee, decimal tax, decimal totalAmount, string currency, string pickupLocation, string deliveryAddress);
    Task SendGenericEmailAsync(string toEmail, string subject, string htmlBody);
    Task SendFirstOnboardingReminderAsync(string toEmail, string name, List<string> pendingItems);
    Task SendSecondOnboardingReminderAsync(string toEmail, string name, List<string> pendingItems);
}