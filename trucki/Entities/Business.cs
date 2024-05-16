namespace trucki.Entities;

public class Business: BaseClass
{
    public string Name { get; set; }
    public string Ntons { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
    public Manager Manager { get; set; }
    public string? managerId { get; set; }
    public List<Routes>? Routes { get; set; }
}