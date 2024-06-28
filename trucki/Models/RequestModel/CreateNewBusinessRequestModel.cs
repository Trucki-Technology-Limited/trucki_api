namespace trucki.Models.RequestModel;

public class CreateNewBusinessRequestModel
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
}