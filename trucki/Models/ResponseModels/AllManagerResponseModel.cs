using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class AllManagerResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public string Phone { get; set; }
    
    public string EmailAddress { get; set; }
    public ManagerType ManagerType { get; set; }
    public bool IsActive { get; set; }
}