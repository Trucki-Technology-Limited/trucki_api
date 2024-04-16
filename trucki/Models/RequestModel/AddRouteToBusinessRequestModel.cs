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

    public string ToRoute { get; set; }

    public float Price { get; set; }

    public bool IsActive { get; set; }

}