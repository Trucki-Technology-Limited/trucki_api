namespace trucki.DTOs
{
    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string FirstName { get; set; }   
        public string TemplateName { get; set; }
    }
}
