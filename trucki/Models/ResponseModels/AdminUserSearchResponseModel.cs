namespace trucki.Models.ResponseModels;

public class AdminUserSearchResponseModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string UserType { get; set; }
    public string UserTypeDisplay { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; } // For cargo owners and truck owners
}