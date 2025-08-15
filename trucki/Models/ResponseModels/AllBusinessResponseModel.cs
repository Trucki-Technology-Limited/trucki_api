namespace trucki.Models.ResponseModels;

public class AllBusinessResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}