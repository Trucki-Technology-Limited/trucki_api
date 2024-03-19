namespace trucki.Entities
{
    public class Company : BaseClass
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string? ManageName { get; set; }

        // Navigation properties
        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
      //  public ICollection<Manager> Managers { get; set; } = new List<Manager>();
    }
}
