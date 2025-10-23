using trucki.Entities;

namespace trucki.Interfaces.IServices;

public interface IDocumentTypeService
{
    Task<DocumentType> GetDocumentTypeAsync(string id);
    Task<IEnumerable<DocumentType>> GetAllDocumentTypesAsync();
    Task<IEnumerable<DocumentType>> GetDocumentTypesByCountryAndEntityAsync(string country, string entityType);
    Task<DocumentType> CreateDocumentTypeAsync(DocumentType documentType);
    Task<DocumentType> UpdateDocumentTypeAsync(DocumentType documentType);
    Task<bool> DeleteDocumentTypeAsync(string id);
}
