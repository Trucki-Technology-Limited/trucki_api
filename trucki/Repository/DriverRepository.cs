using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Repository;

namespace trucki.Repository
{
    public class DriverRepository : RepositoryBase<Driver>, IDriverRepository
    {
        public DriverRepository(TruckiDBContext repositoryContext) : base(repositoryContext)
        { }

        public void CreateTruckiDriver(Driver driver) => Create(driver);
        public void UpdateTruckiDrivern(Driver driver) => Update(driver);
        public void DeleteTruckiDriver(Driver driver) => Delete(driver);

        public async Task<IEnumerable<Driver>> FetchTruckiDrivers(DriverParameter driverParameter) => await FindAll(false)
        .OrderByDescending(e => e.CreatedAt)
        .Skip((driverParameter.PageNumber - 1) * driverParameter.PageSize)
        .Take(driverParameter.PageSize)
        .ToListAsync();

        public async Task<Driver> FetchDriverById(string driverId, bool trackChanges) => await FindByCondition(x => x.Id.ToLower().Equals(driverId.ToLower()), trackChanges).FirstOrDefaultAsync();

        public async Task SaveAsync() => await SaveAsync();
    }
}


