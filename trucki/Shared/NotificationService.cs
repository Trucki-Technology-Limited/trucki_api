
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
                //var body = mailRequest.TemplateName;
                var mailTemplate = mailRequest.TemplateName.Replace("{Firstname}", mailRequest.FirstName)
                                   .Replace("{Message}", mailRequest.Message);
                var body = File.ReadAllText(mailTemplate);
               

                var message = new JObject
                {
                    {
                        "From", new JObject
                        {
                            {"Email", "sheyeogunsanmi@gmail.com"},
                            {"Name", "UserTrucki"}
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
                    /*_logger.LogInformation(string.Format("Total: {0}, Count: {1}\n", mailjetResponse.GetTotal(), mailjetResponse.GetCount()));
                    _logger.LogInformation(mailjetResponse.GetData().ToString());
                    response = true;*/

                    var responseData = mailjetResponse.GetData().ToString();
                    try
                    {
                        // Parse the JSON response using Newtonsoft.Json
                        var parsedData = JsonConvert.DeserializeObject<MailjetResponse>(responseData);

                        // Process parsedData as needed
                        _logger.LogInformation(JsonConvert.SerializeObject(parsedData));
                        response = true;
                    }
                    catch (JsonReaderException ex)
                    {
                        // Handle JSON parsing errors
                        _logger.LogError($"Error parsing JSON response: {ex.Message}");
                        // You can choose to return false or handle the error differently
                        response = false;
                    }
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
