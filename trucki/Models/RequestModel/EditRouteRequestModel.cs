namespace trucki.Models.RequestModel;

public class EditRouteRequestModel
{
    public string Id { get; set; } // Route ID
    public string Name { get; set; }
    public string FromRoute { get; set; }
    public string ToRoute { get; set; }
    public float Price { get; set; }
    public bool IsActive { get; set; }
    public string Gtv { get; set; }
}