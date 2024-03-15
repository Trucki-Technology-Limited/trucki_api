namespace trucki.DTOs
{
    public class MailRequestDto
    {
        public string Path { get; set; }
        public string Message { get; set; }
        public string FirstName { get; set; }
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
    }
}
