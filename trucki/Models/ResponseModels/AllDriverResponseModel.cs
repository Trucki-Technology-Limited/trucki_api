using System.ComponentModel.DataAnnotations;

namespace trucki.Models.ResponseModels
{
    public class AllDriverResponseModel
    {
        public string Id { get; set; }  
        public string Name { get; set; }

        public string Phone { get; set; }

        public string EmailAddress { get; set; }

        public string TruckId { get; set; }

        public string DriversLicence { get; set; }

        public string PassportFile { get; set; }
    }
    
    public class DriverResponseModel
    {
        public string Id { get; set; }  
        public string Name { get; set; }

        public string Phone { get; set; }

        public string EmailAddress { get; set; }

        public string TruckId { get; set; }

        public string DriversLicence { get; set; }
        public AllTruckResponseModel Truck { get; set; }

        public string PassportFile { get; set; }
    }
}
