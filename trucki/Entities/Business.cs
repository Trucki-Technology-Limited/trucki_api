using System.Globalization;

namespace trucki.Entities;

public class Business: BaseClass
{
    public string Name { get; set; }
    public string Ntons { get; set; }
    public string Address { get; set; }
    public bool isActive { get; set; }
    public List<Routes>? Routes { get; set; }
}