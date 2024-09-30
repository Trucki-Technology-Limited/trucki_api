namespace trucki.Models.RequestModel;

public class AddTruckOwnerRequestBody
{
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public string IdCard  { set; get; }
    public string ProfilePicture { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
}

public class AddTransporterRequestBody
{
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string? Phone { set; get; }
    public string Address { set; get; }
    public string password { set; get; }
}
public class EditTruckOwnerRequestBody
{
    public string Id { set; get; }
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public string? IdCard  { set; get; }
}