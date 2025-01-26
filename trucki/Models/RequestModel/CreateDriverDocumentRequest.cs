using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    public class CreateDriverDocumentRequest
    {
        [Required]
        public string DriverId { get; set; }

        [Required]
        public string DocumentTypeId { get; set; }

        [Required]
        [Url]  // or any custom validation for file location
        public string FileUrl { get; set; }
    }
}
