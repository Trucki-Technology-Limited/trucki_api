using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;

namespace trucki.Services;

public class EmailService: IEmailSender
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

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
                var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Trucki Limited", _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;

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