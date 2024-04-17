namespace trucki.Models.RequestModel
{
    public class AddCustomerRequestModel
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Location { get; set; }
        public string? RCNo { get; set; }
    }

    public class EditCustomerRequestModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Location { get; set; }
        public string? RCNo { get; set; }
    }
}
