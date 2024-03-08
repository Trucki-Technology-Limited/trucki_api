using trucki.DTOs;
using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface IManagerRepository
    {
        void CreateTruckiManager(Manager manager);
        void UpdateTruckiManagern(Manager manager);
        void DeleteTruckiManager(Manager manager);
        Task<IEnumerable<Manager>> FetchTruckiManagers(ManagerParameter managerParameter);
        Task<Manager> FetchManagerById(string managerId, bool trackChanges);
        Task SaveAsync();
    }
}
