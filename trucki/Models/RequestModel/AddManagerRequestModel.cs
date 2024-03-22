using trucki.Entities;

namespace trucki.Models.RequestModel;

public class AddManagerRequestModel
{
    public string Name { get; set; }
    public int Phone { get; set; }
    public string EmailAddress { get; set; }
    public string Company { get; set; }
    public ManagerType ManagerType { get; set; }
}