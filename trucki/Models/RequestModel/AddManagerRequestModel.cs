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