using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel;

public class AdminUserSearchRequestModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    public string? SearchQuery { get; set; }

    public string? UserType { get; set; } // driver, cargoowner, truckowner, manager

    public bool? IsActive { get; set; } // filter by active status
}