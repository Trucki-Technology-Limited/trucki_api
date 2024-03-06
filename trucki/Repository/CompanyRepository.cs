using trucki.DatabaseContext;
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
        public async Task SaveAsync() => await SaveAsync();
    }
}
    