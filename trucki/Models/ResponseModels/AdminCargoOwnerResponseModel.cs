using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class AdminCargoOwnerResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string Phone { get; set; }
    public string CompanyName { get; set; }
    public string Address { get; set; }
    public string Country { get; set; }
    public CargoOwnerType OwnerType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermDays { get; set; }
}

public class AdminCargoOwnerDetailsResponseModel : AdminCargoOwnerResponseModel
{
    public int TotalCompletedOrdersCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public int InTransitOrdersCount { get; set; }
    public decimal TotalOrderValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
}