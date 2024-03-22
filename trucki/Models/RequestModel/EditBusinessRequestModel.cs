namespace trucki.Models.RequestModel;

public class EditBusinessRequestModel
{
    public string Id { get; set; } // Business ID
    public string Name { get; set; }
    public string Ntons { get; set; }
    public string Address { get; set; }
    public bool IsActive { get; set; }
}