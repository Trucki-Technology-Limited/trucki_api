using HandlebarsDotNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using trucki.Interfaces.IServices;

namespace trucki.Services;

public class EmailService : IEmailService
{

    private readonly string _Email;
    private readonly string _Password;
    private readonly string _fromEmail;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly bool _useSsl;


    public EmailService(IConfiguration configuration)
    {
        _Email = configuration["EmailSetting:Email"];
        _Password = configuration["EmailSetting:Password"];
        _fromEmail = configuration["EmailSetting:From"];
        _smtpServer = configuration["EmailSetting:SmtpServer"];
        _smtpPort = int.Parse(configuration["EmailSetting:SmtpPort"]);
        _useSsl = bool.Parse(configuration["EmailSetting:UseSsl"]);
    }

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
    }
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
        string supportEmail = "info@trucki.com";
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
                supportEmail = "support@trucki.com";
                steps = new List<string>
                {
                    "Post Your First Load – Set pickup & delivery details in minutes.",
                    "Get Matched with Verified Drivers – AI-powered load matching for efficiency & cost savings.",
                    "Track Shipments in Real Time – Monitor every step with GPS tracking."
                };
                break;

            case "truck owner":
                subject = "Welcome to Trucki! Confirm Your Email & Start Managing Your Fleet";
                welcomeMessage = "Welcome to Trucki, your all-in-one platform for fleet management and cargo fulfillment!";
                actionText = "assigning loads to your drivers";
                thankYouText = "joining Trucki, let's build a smarter, more connected fleet together!";
                supportEmail = "info@trucki.com";
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
}