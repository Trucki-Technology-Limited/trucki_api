namespace trucki.Models.RequestModel;

public class AcceptOrderRequestModel
{
    public string orderId { get; set; }
    public string driverId { get; set; }
    public string status { get; set; }
}