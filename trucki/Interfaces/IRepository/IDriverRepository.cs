using trucki.DTOs;
using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface IDriverRepository
    {
        void CreateTruckiDriver(Driver driver);
        void UpdateTruckiDrivern(Driver driver);
        void DeleteTruckiDriver(Driver driver);
        Task<Driver> FetchDriverById(string driverId, bool trackChanges);
        Task<IEnumerable<Driver>> FetchTruckiDrivers(DriverParameter driverParameter);
        Task SaveAsync();
    }
}
