namespace trucki.Models.RequestModel;

public class AddRouteToBusinessRequestModel
{
    public List<RouteToBusinessRequestModel> Routes { set; get; }
    public string BusinessId { get; set; }
}


public class RouteToBusinessRequestModel
{
    public string Name { get; set; }

    public string FromRoute { get; set; }
    public string? FromRouteLat { get; set; }
    public string? FromRouteLng { get; set; }

    public string ToRoute { get; set; }
    public string? ToRouteLat { get; set; }
    public string? ToRouteLng { get; set; }
    public string Ntons { get; set; }

    public float Price { get; set; }

    public bool IsActive { get; set; }
    public float Gtv { get; set; }

}