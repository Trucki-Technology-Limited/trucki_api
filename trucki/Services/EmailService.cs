using HandlebarsDotNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using trucki.Interfaces.IServices;

namespace trucki.Services;

public class EmailService: IEmailService
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
            var templatePath = Path.Combine(AppContext.BaseDirectory, "otp_template.html"); // Using BaseDirectory instead of GetCurrentDirectory
            // var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "otp_template.html"); // Assuming the template is in your project's root
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
        
        public async Task SendOrderEmailAsync(string toEmail, string subject, string date, string orderId, string status,string businessName,string deliveryAddress)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "order_template.html"); // Assuming the template is in your project's root
            var templateSource = File.ReadAllText(templatePath);
            var template = Handlebars.Compile(templateSource);

            //      // 3. Render the template with the OTP
            var htmlBody = template(new { date=date,orderId=orderId,status=status,businessName =businessName,deliveryAddress=deliveryAddress });
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
}