using System.Threading.Tasks;
using trucki.DTOs;
using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface IBusinessRepository
    {
        void CreateTruckiBusiness(Business business);
        void UpdateTruckiBusiness(Business business);
        void DeleteTruckiBusiness(Business business);
        Task<IEnumerable<Business>> FetchAllTruckiBusinesses(BusinessParameter businessParameter);
        Task<Business> FetchBusinessById(string businessId, bool trackChanges);
        Task SaveAsync();
    }
}
