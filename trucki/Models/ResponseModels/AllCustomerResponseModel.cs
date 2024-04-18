namespace trucki.Models.ResponseModels
{
    public class AllCustomerResponseModel
    {
        public string Id { get; set; }  
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Location { get; set; }
        public string? RCNo { get; set; }   
    }
}
