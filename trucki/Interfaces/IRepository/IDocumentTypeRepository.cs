using trucki.Entities;

namespace trucki.Interfaces.IRepository;

public interface IDocumentTypeRepository
{
    Task<DocumentType> GetByIdAsync(string id);
    Task<IEnumerable<DocumentType>> GetAllAsync();
    Task<DocumentType> CreateAsync(DocumentType documentType);
    Task<DocumentType> UpdateAsync(DocumentType documentType);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<DocumentType>> GetAllRequiredForCountryAndEntityAsync(string country, string entityType);
    Task<IEnumerable<DocumentType>> GetByCountryAndEntityTypeAsync(string country, string entityType);
}
