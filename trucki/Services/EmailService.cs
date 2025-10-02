using HandlebarsDotNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using trucki.Interfaces.IServices;

namespace trucki.Services;

public class EmailService : IEmailService
{
    /// <summary>
    /// EmailService handles all outbound email functionalities, including sending OTPs, order notifications,
    /// password resets, welcome emails, and payment receipts via SMTP. Since the official Resend SDK requires .NET 8,
    /// we implement a custom SMTP client approach here for .NET 6 compatibility.
    /// 
    /// Note: As of April 2025, accessing Resend's platform requires VPN access. Make sure to connect to the VPN
    /// before attempting to configure or send emails.
    /// </summary>

    private readonly string _Email;
    private readonly string _Password;
    private readonly string _fromEmail;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly bool _useSsl;
    /// <summary>
    /// Constructor retrieves email configuration from app settings. We store Resend SMTP
    /// credentials and server configuration in the application's configuration file.
    /// 
    /// For example, in appsettings.json:
    /// "EmailSetting": {
    ///   "Email": "your_resend_smtp_username",
    ///   "Password": "your_resend_smtp_password",
    ///   "From": "no-reply@yourdomain.com",
    ///   "SmtpServer": "smtp.resend.com",  // Example host
    ///   "SmtpPort": "587",
    ///   "UseSsl": "true"
    /// }
    /// </summary>
    /// <param name="configuration">App configuration injected by the DI container.</param>


    public EmailService(IConfiguration configuration)
    {
        _Email = configuration["EmailSetting:Email"];
        _Password = configuration["EmailSetting:Password"];
        _fromEmail = configuration["EmailSetting:From"];
        _smtpServer = configuration["EmailSetting:SmtpServer"];
        _smtpPort = int.Parse(configuration["EmailSetting:SmtpPort"]);
        _useSsl = bool.Parse(configuration["EmailSetting:UseSsl"]);
    }
    /// <summary>
    /// Sends a One-Time Password (OTP) email to the specified recipient using an HTML template.
    /// 
    /// Note: The 'otp_template.html' file should be placed in the root of the project.
    /// </summary>
    /// <param name="toEmail">Recipient's email address.</param>
    /// <param name="subject">Email subject line (e.g. "Your OTP Code").</param>
    /// <param name="otp">The OTP value to be included in the email template.</param>

    public async Task SendEmailAsync(string toEmail, string subject, string otp)
    {
        // var templatePath = Path.Combine(AppContext.BaseDirectory, "otp_template.html"); // Using BaseDirectory instead of GetCurrentDirectory
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "otp_template.html"); // Assuming the template is in your project's root
        var templateSource = File.ReadAllText(templatePath);
        var template = Handlebars.Compile(templateSource);

        //      // 3. Render the template with the OTP
        var htmlBody = template(new { otp = otp });
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Trucki Limited", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlBody;

        message.Body = bodyBuilder.ToMessageBody();
        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_Email, _Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
    /// <summary>
    /// Sends an order notification email using an HTML template.
    /// </summary>
    /// <param name="toEmail">Recipient's email address.</param>
    /// <param name="subject">Subject line for the email.</param>
    /// <param name="date">Date of order or notification.</param>
    /// <param name="orderId">Unique identifier for the order.</param>
    /// <param name="status">Status of the order (e.g., "Processing").</param>
    /// <param name="businessName">Name of the business for the email template.</param>
    /// <param name="deliveryAddress">Delivery address for the order.</param>

    public async Task SendOrderEmailAsync(string toEmail, string subject, string date, string orderId, string status, string businessName, string deliveryAddress)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "order_template.html"); // Assuming the template is in your project's root
        var templateSource = File.ReadAllText(templatePath);
        var template = Handlebars.Compile(templateSource);

        //      // 3. Render the template with the OTP
        var htmlBody = template(new { date = date, orderId = orderId, status = status, businessName = businessName, deliveryAddress = deliveryAddress });
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Trucki Limited", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlBody;
        message.Body = bodyBuilder.ToMessageBody();
        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_Email, _Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }    /// <summary>
         /// Sends a password reset email using a reset code embedded into an HTML template.
         /// </summary>

    public async Task SendPasswordResetEmailAsync(string toEmail, string subject, string resetCode)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "password_reset_template.html");
        var templateSource = File.ReadAllText(templatePath);
        var template = Handlebars.Compile(templateSource);

        var htmlBody = template(new { resetCode = resetCode });
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Trucki Limited", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlBody;

        message.Body = bodyBuilder.ToMessageBody();
        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_Email, _Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
    /// <summary>
    /// Sends a welcome email tailored to different user types (Driver, Cargo Owner, or Truck Owner).
    /// The email body is generated from a Handlebars template.
    /// </summary>


    public async Task SendWelcomeEmailAsync(string toEmail, string name, string userType, string confirmationLink)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "welcome_email_template.html");
        var templateSource = File.ReadAllText(templatePath);
        var template = Handlebars.Compile(templateSource);

        // Default values
        string subject = "Welcome to Trucki! Confirm Your Email & Start Your Journey";
        string welcomeMessage = "Welcome to Trucki, the smarter way to connect with loads and keep your wheels moving!";
        string actionText = "accepting trips and maximizing your earnings";
        string thankYouText = "joining Trucki, let's hit the road together!";
        string supportEmail = "info@trucki.co";
        string helpType = "help";
        List<string> steps = new List<string>
        {
            "Complete Your Profile – Add your vehicle details, documents & payment info.",
            "Find & Accept Loads – Get the best-paying trips.",
            "Track & Get Paid Fast – Real-time trip monitoring & weekly payouts."
        };

        // Customize based on user type
        switch (userType.ToLower())
        {
            case "driver":
                // Default values are already set for drivers
                break;

            case "cargo owner":
                subject = "Welcome to Trucki! Confirm Your Email & Start Shipping";
                welcomeMessage = "Welcome to Trucki, your trusted platform for seamless freight shipping!";
                actionText = "posting loads and connecting with reliable drivers";
                thankYouText = "choosing Trucki, we're here to power your logistics!";
                supportEmail = "support@trucki.co";
                steps = new List<string>
                {
                    "Post Your First Load – Set pickup & delivery details in minutes.",
                    "Get Matched with Verified Drivers – AI-powered load matching for efficiency & cost savings.",
                    "Track Shipments in Real Time and Monitor every step with GPS tracking."
                };
                break;

            case "truck owner":
                subject = "Welcome to Trucki! Confirm Your Email & Start Managing Your Fleet";
                welcomeMessage = "Welcome to Trucki, your all-in-one platform for fleet management and cargo fulfillment!";
                actionText = "assigning loads to your drivers";
                thankYouText = "joining Trucki, let's build a smarter, more connected fleet together!";
                supportEmail = "info@trucki.co";
                steps = new List<string>
                {
                    "Add Your Fleet & Drivers – Register trucks & onboard drivers seamlessly.",
                    "Get Load Assignments – AI-powered load matching for maximum efficiency.",
                    "Track Performance & Earnings – Real-time trip monitoring & fast payments."
                };
                break;
        }

        var htmlBody = template(new
        {
            name = name,
            email = toEmail,
            welcomeMessage = welcomeMessage,
            actionText = actionText,
            confirmationLink = confirmationLink,
            steps = steps,
            helpType = helpType,
            supportEmail = supportEmail,
            thankYouText = thankYouText
        });

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Trucki Limited", _fromEmail));
        message.To.Add(new MailboxAddress(name, toEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlBody;

        message.Body = bodyBuilder.ToMessageBody();
        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_Email, _Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
    /// <summary>
    /// Sends a payment receipt email containing a summarized invoice, including fees, taxes, and other
    /// monetary details. It uses an HTML template to format the receipt.
    /// </summary>

    public async Task SendPaymentReceiptEmailAsync(string toEmail, string orderId, decimal bidAmount, decimal systemFee, decimal tax, decimal totalAmount, string currency, string pickupLocation, string deliveryAddress)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "invoice_email_template.html");
        var templateSource = File.ReadAllText(templatePath);
        var template = Handlebars.Compile(templateSource);

        var date = DateTime.UtcNow.ToString("MMMM dd, yyyy");
        var status = "Payment Successful";

        var htmlBody = template(new
        {
            date,
            orderId,
            status,
            bidAmount,
            systemFee,
            tax,
            totalAmount,
            currency,
            pickupLocation,
            deliveryAddress
        });

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Trucki Limited", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = $"Payment Receipt: {pickupLocation} to {deliveryAddress}";
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlBody;

        message.Body = bodyBuilder.ToMessageBody();
        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_Email, _Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}