using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repository;

public class DocumentTypeRepository : IDocumentTypeRepository
{
    private readonly TruckiDBContext _dbContext;

    public DocumentTypeRepository(TruckiDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DocumentType> GetByIdAsync(string id)
    {
        return await _dbContext.DocumentTypes.FindAsync(id);
    }

    public async Task<IEnumerable<DocumentType>> GetAllAsync()
    {
        return await _dbContext.DocumentTypes.ToListAsync();
    }

    public async Task<DocumentType> CreateAsync(DocumentType documentType)
    {
        _dbContext.DocumentTypes.Add(documentType);
        await _dbContext.SaveChangesAsync();
        return documentType;
    }

    public async Task<DocumentType> UpdateAsync(DocumentType documentType)
    {
        // EF tracks the entity if it's attached. If not attached, you may need to attach it.
        _dbContext.DocumentTypes.Update(documentType);
        await _dbContext.SaveChangesAsync();
        return documentType;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await _dbContext.DocumentTypes.FindAsync(id);
        if (existing == null)
            return false;

        _dbContext.DocumentTypes.Remove(existing);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<IEnumerable<DocumentType>> GetAllRequiredForCountryAndEntityAsync(string country, string entityType)
    {
        return await _dbContext.DocumentTypes
            .Where(dt => dt.Country == country
                         && dt.EntityType == entityType)
            .ToListAsync();
    }
}
