namespace trucki.Models.ResponseModels;

public class RouteResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string FromRoute { get; set; }

    public string ToRoute { get; set; }

    public float Price { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}