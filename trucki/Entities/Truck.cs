namespace trucki.Entities;

public class Truck
{
    public string CertOfOwnerShip { set; get; }
    public TruckiType TruckType { set; get; }
}

public enum TruckiType
{
    Flatbed,
    BoxBody,
    BucketBody,
    Lowbed,
    ContainerizedBody,
    Refrigerator
}
