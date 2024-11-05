namespace trucki.Models.ResponseModels;

public class RouteResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string FromRoute { get; set; }
    public string? FromRouteLat { get; set; }
    public string? FromRouteLng { get; set; }

    public string ToRoute { get; set; }
    public string? ToRouteLat { get; set; }
    public string? ToRouteLng { get; set; }

    public float Price { get; set; }
    public string Ntons { get; set; }
    public string Gtv { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}