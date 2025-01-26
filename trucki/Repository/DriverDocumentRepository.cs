using Microsoft.EntityFrameworkCore;      // Your EF DbContext namespace
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repositories
{
    public class DriverDocumentRepository : IDriverDocumentRepository
    {
        private readonly TruckiDBContext _dbContext;

        public DriverDocumentRepository(TruckiDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DriverDocument> GetByIdAsync(string id)
        {
            return await _dbContext.DriverDocuments
                .Include(dd => dd.DocumentType)
                .Include(dd => dd.Driver)
                .FirstOrDefaultAsync(dd => dd.Id == id);
        }

        public async Task<IEnumerable<DriverDocument>> GetAllForDriverAsync(string driverId)
        {
            return await _dbContext.DriverDocuments
                .Include(dd => dd.DocumentType)
                .Where(dd => dd.DriverId == driverId)
                .ToListAsync();
        }

        public async Task<DriverDocument> CreateAsync(DriverDocument driverDocument)
        {
            _dbContext.DriverDocuments.Add(driverDocument);
            await _dbContext.SaveChangesAsync();
            return driverDocument;
        }

        public async Task<DriverDocument> UpdateAsync(DriverDocument driverDocument)
        {
            _dbContext.DriverDocuments.Update(driverDocument);
            await _dbContext.SaveChangesAsync();
            return driverDocument;
        }

        public async Task<bool> DeleteAsync(string driverDocumentId)
        {
            var existing = await _dbContext.DriverDocuments.FindAsync(driverDocumentId);
            if (existing == null)
                return false;

            _dbContext.DriverDocuments.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
