
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using trucki.DTOs;

namespace trucki.Shared
{
    public class NotificationService : INotificationService
    {
        private readonly IMailjetClient _client;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IMailjetClient client, ILogger<NotificationService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(MailRequest mailRequest)
        {
            try
            {
                var response = false;
                var body = mailRequest.TemplateName.Replace("{Firstname}", mailRequest.FirstName)
                                    .Replace("{Message}", mailRequest.Message);

                var message = new JObject
                {
                    {
                        "From", new JObject
                        {
                            {"Email", "sheyeogunsanmi@gmail.com"},
                            {"Name", "Trucki"}
                        }
                    },
                    {
                        "To", new JArray
                        {
                            new JObject
                            {
                                {"Email", mailRequest.ToEmail},
                            }
                        }
                    },
                    {"Subject", mailRequest.Subject},
                    {"HtmlPart", body},
                    {"CustomId", "AppGettingStartedTest"}
                };


                MailjetRequest request = new MailjetRequest { Resource = SendV31.Resource }
                    .Property(Send.Messages, new JArray { message });

                MailjetResponse mailjetResponse = await _client.PostAsync(request);
                if (mailjetResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation(string.Format("Total: {0}, Count: {1}\n", mailjetResponse.GetTotal(), mailjetResponse.GetCount()));
                    _logger.LogInformation(mailjetResponse.GetData().ToString());
                    response = true;


                }
                return response;
            }

            catch (Exception)
            {
                throw;
            }
        }
    }
}
