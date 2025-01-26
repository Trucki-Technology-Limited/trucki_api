using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface IDriverDocumentRepository
    {
        Task<DriverDocument> GetByIdAsync(string id);
        Task<IEnumerable<DriverDocument>> GetAllForDriverAsync(string driverId);
        Task<DriverDocument> CreateAsync(DriverDocument driverDocument);
        Task<DriverDocument> UpdateAsync(DriverDocument driverDocument);
        Task<bool> DeleteAsync(string driverDocumentId);
    }
}
