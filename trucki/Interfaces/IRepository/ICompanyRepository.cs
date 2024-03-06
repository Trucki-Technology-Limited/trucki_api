using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface ICompanyRepository
    {
       void CreateTruckiCompanies(Company company);
       void UpdateTruckiCompanies(Company company);
       Task SaveAsync();
    }
}
