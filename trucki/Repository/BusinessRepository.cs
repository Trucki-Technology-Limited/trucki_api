using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repository
{
    public class BusinessRepository : RepositoryBase<Business>, IBusinessRepository
    {
        public BusinessRepository(TruckiDBContext repositoryContext) : base(repositoryContext)
        { }

        public void CreateTruckiBusiness(Business business) => Create(business);
        public void UpdateTruckiBusiness(Business business) => Update(business);
        public void DeleteTruckiBusiness(Business business) => Delete(business);

        public async Task<IEnumerable<Business>> FetchAllTruckiBusinesses(BusinessParameter businessParameter) => await FindAll(false)
        .OrderByDescending(e => e.CreatedAt)
        .Skip((businessParameter.PageNumber - 1) * businessParameter.PageSize)
        .Take(businessParameter.PageSize)
        .ToListAsync();

        public async Task<Business> FetchBusinessById(string businessId, bool trackChanges) => await FindByCondition(x => x.Id.ToLower().Equals(businessId.ToLower()), trackChanges).FirstOrDefaultAsync();

        public async Task SaveAsync() => await SaveChangesAsync();
    }
}
