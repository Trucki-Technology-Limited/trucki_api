using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    public class SubmitRatingDto
    {
        [Required]
        public string CargoOrderId { get; set; }
        [Required]
        public string CargoOwnerId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}