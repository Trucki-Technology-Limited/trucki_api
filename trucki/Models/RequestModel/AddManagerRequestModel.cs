using trucki.Entities;

namespace trucki.Models.RequestModel;

public class AddManagerRequestModel
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string EmailAddress { get; set; }
    public List<string> CompanyId { get; set; }
    public ManagerType ManagerType { get; set; }
}

public class EditAssignedBusinessesRequestModel
{
    public string ManagerId { get; set; } // Manager's ID
    public List<string> CompanyIds { get; set; } // List of business IDs
}

public class GetTransactionsByManagerRequestModel
{
    // Pagination parameters
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Optional filtering
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TransactionType? TransactionType { get; set; }
    public string? BusinessId { get; set; }
    public string? SearchTerm { get; set; } // For searching by business name, truck owner, etc.
    
    // Optional sorting
    public string? SortBy { get; set; } = "TransactionDate";
    public bool SortDescending { get; set; } = true;
    
    // Method to validate and normalize parameters
    public void ValidateAndNormalize()
    {
        PageNumber = PageNumber < 1 ? 1 : PageNumber;
        PageSize = PageSize < 1 ? 10 : PageSize > 100 ? 100 : PageSize;
        
        if (string.IsNullOrWhiteSpace(SortBy))
            SortBy = "TransactionDate";
    }
}