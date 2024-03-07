namespace trucki.Entities
{
    public class TruckOwner : BaseClass
    {
        public string CompanyName { get; set; } 
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? TruckType { get; set; }
    }
}
