using trucki.DTOs;
using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface ICompanyRepository
    {
       void CreateTruckiCompanies(Company company);
       void UpdateTruckiCompanies(Company company);
       Task<IEnumerable<Company>> FetchAllTruckiCompanies(CompanyParameter companyParameter);
       Task<Company> FetchComapnyById(string managerId, bool trackChanges);
       Task SaveAsync();
    }
}
