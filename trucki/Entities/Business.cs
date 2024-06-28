namespace trucki.Entities;

public class Business: BaseClass
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
    public Manager Manager { get; set; }
    public string? managerId { get; set; }
    public List<Routes>? Routes { get; set; }
    public List<Customer>? Customers { get; set; }
}