using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class Driver : BaseClass
    {
        public string Name { get; set; }

        // this is suppose to string not int
        //[MaxLength(11)]
        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Company { get; set; }

        public string? DriversLicence { get; set; }

        public string? PassportFile { get; set; }
    }
}
