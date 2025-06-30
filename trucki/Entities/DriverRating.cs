using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class DriverRating : BaseClass
    {
        [Required]
        public string CargoOrderId { get; set; }
        public CargoOrders CargoOrder { get; set; }

        [Required]
        public string DriverId { get; set; }
        public Driver Driver { get; set; }

        [Required]
        public string CargoOwnerId { get; set; }
        public CargoOwner CargoOwner { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime RatedAt { get; set; } = DateTime.UtcNow;
    }
}