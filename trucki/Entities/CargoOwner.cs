using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities;

public class CargoOwner : BaseClass
{
    public string Name { get; set; }

    public string Phone { get; set; }

    [EmailAddress]
    public string EmailAddress { get; set; }
    public string CompanyName { get; set; }
    public string Address { get; set; }
    [ForeignKey("User")]
    public string? UserId { get; set; }

    public User? User { get; set; }

    // New property to capture which country this driver belongs to (if relevant)
    public string Country { get; set; } = "NG";  // or "NG" or any default

    public List<CargoOrders> Orders { get; set; }
}