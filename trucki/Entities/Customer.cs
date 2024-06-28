namespace trucki.Entities
{
    public class Customer : BaseClass
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Location { get; set; }
        public string? RCNo { get; set; }
        public string BusinessId { get; set; } // Foreign key for Business
        public Business Business { get; set; }
    }
}
