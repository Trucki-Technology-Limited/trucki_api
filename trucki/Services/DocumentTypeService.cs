using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;

namespace trucki.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;

    public DocumentTypeService(IDocumentTypeRepository documentTypeRepository)
    {
        _documentTypeRepository = documentTypeRepository;
    }

    public async Task<DocumentType> GetDocumentTypeAsync(string id)
    {
        // You could add extra logic (e.g., check if user is authorized)
        return await _documentTypeRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<DocumentType>> GetAllDocumentTypesAsync()
    {
        return await _documentTypeRepository.GetAllAsync();
    }

    public async Task<DocumentType> CreateDocumentTypeAsync(DocumentType documentType)
    {
        // Example: Validate or transform the input
        // if (string.IsNullOrWhiteSpace(documentType.Name))
        // {
        //     throw new ArgumentException("Document name is required.");
        // }
        // Example validation: if HasTemplate is true, ensure TemplateUrl is set
        if (documentType.HasTemplate && string.IsNullOrWhiteSpace(documentType.TemplateUrl))
        {
            throw new ArgumentException("TemplateUrl must be provided if HasTemplate is true.");
        }
        return await _documentTypeRepository.CreateAsync(documentType);
    }

    public async Task<DocumentType> UpdateDocumentTypeAsync(DocumentType documentType)
    {
        // Possibly validate or do business checks
        return await _documentTypeRepository.UpdateAsync(documentType);
    }

    public async Task<bool> DeleteDocumentTypeAsync(string id)
    {
        return await _documentTypeRepository.DeleteAsync(id);
    }
}
