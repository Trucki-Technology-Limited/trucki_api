using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repository
{
    public class ManagerRepository : RepositoryBase<Manager>, IManagerRepository
    {
        public ManagerRepository(TruckiDBContext repositoryContext) : base(repositoryContext)
        { }

        public void CreateTruckiManager(Manager manager) => Create(manager);
        public void UpdateTruckiManagern(Manager manager) => Update(manager);
        public void DeleteTruckiManager(Manager manager) => Delete(manager);

        public async Task<IEnumerable<Manager>> FetchTruckiManagers(ManagerParameter managerParameter) => await FindAll(false)
        .OrderByDescending(e => e.CreatedAt)
        .Include(e => e.Company)
        .Skip((managerParameter.PageNumber - 1) * managerParameter.PageSize)
        .Take(managerParameter.PageSize)
        .ToListAsync();

        public async Task<Manager> FetchManagerById(string managerId, bool trackChanges) => await FindByCondition(x => x.Id.ToLower().Equals(managerId.ToLower()), trackChanges)
            .Include(x => x.Company)
            .FirstOrDefaultAsync();

        public async Task SaveAsync() => await SaveChangesAsync();
    }
}
