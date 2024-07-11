using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class AllManagerResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public string Phone { get; set; }
    
    public string EmailAddress { get; set; }
    public List<ManagersBusinessResponseModel> Company { get; set; }
    public ManagerType ManagerType { get; set; }
    public bool IsActive { get; set; }
}


public class ManagersBusinessResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}