namespace trucki.Interfaces.IServices;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string otp);
    Task SendOrderEmailAsync(string toEmail, string subject, string date, string orderId, string status, string businessName, string deliveryAddress);
    Task SendPasswordResetEmailAsync(string toEmail, string subject, string resetCode);
    Task SendWelcomeEmailAsync(string toEmail, string name, string userType, string confirmationLink);
}