using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class BaseClass
    {
        [Key]

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
