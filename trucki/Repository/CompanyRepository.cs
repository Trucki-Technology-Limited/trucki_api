using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(TruckiDBContext context) : base(context)
        {
        }

        public void CreateTruckiCompanies(Company company) => Create(company);
        public void UpdateTruckiCompanies(Company company) => Update(company);
        public async Task<IEnumerable<Company>> FetchAllTruckiCompanies(CompanyParameter companyParameter) => await FindAll(false)
          .OrderByDescending(e => e.CreatedAt)
          .Include(e => e.Managers)
          .Skip((companyParameter.PageNumber - 1) * companyParameter.PageSize)
          .Take(companyParameter.PageSize)
          .ToListAsync();

        public async Task<Company> FetchComapnyById(string managerId, bool trackChanges) => await FindByCondition(x => x.Id.ToLower().Equals(managerId.ToLower()), trackChanges)
            .Include(x => x.Managers)
            .FirstOrDefaultAsync();
        public async Task SaveAsync() => await SaveChangesAsync();
    }
}
    