using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.Models.RequestModel;

public class CreateCargoOwnerRequestModel
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    [Phone]
    public string Phone { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string CompanyName { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    public string Country { get; set; } = "NG";
}

public class EditCargoOwnerRequestModel
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    [Phone]
    public string Phone { get; set; }
    
    [Required]
    public string CompanyName { get; set; }
    
    [Required]
    public string Address { get; set; }
}

public class GetCargoOwnerOrdersQueryDto
{
    public string CargoOwnerId { get; set; }
    public CargoOrderStatus? Status { get; set; }
    public string SortBy { get; set; } = "date"; // "date" or "status"
    public bool SortDescending { get; set; } = true;
}