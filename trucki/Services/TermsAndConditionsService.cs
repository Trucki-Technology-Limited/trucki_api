using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;

namespace trucki.Services;

public class TermsAndConditionsService : ITermsAndConditionsService
{
    private readonly ITermsAndConditionsRepository _repository;

    public TermsAndConditionsService(ITermsAndConditionsRepository repository)
    {
        _repository = repository;
    }


    public async Task<IEnumerable<TermsAndConditions>> GetAllTermsAndConditionsAsync()
    {
        return await _repository.GetAllTermsAndConditionsAsync();
    }

    public async Task<IEnumerable<TermsAndConditions>> GetTermsAndConditionsByDocumentTypeAsync(string documentType)
    {
        return await _repository.GetTermsAndConditionsByDocumentTypeAsync(documentType);
    }

    public async Task<TermsAndConditions> GetTermsAndConditionsByIdAsync(string id)
    {
        return await _repository.GetTermsAndConditionsByIdAsync(id);
    }

    public async Task<TermsAndConditions> GetCurrentTermsAndConditionsAsync(string documentType)
    {
        return await _repository.GetCurrentTermsAndConditionsAsync(documentType);
    }

    public async Task<IEnumerable<TermsAndConditions>> GetAllCurrentTermsAndConditionsAsync()
    {
        return await _repository.GetAllCurrentTermsAndConditionsAsync();
    }

    public async Task<TermsAndConditions> CreateTermsAndConditionsAsync(TermsAndConditions termsAndConditions)
    {
        // If this is marked as current version, update the existing current version
        if (termsAndConditions.IsCurrentVersion)
        {
            await SetDocumentTypeAsNotCurrentAsync(termsAndConditions.DocumentType);
        }

        return await _repository.CreateTermsAndConditionsAsync(termsAndConditions);
    }

    public async Task<TermsAndConditions> UpdateTermsAndConditionsAsync(TermsAndConditions termsAndConditions)
    {
        var existingTerms = await _repository.GetTermsAndConditionsByIdAsync(termsAndConditions.Id);
        if (existingTerms == null)
        {
            throw new KeyNotFoundException($"Terms and Conditions with ID {termsAndConditions.Id} not found.");
        }

        // If we're making this the current version and it wasn't before,
        // or if the document type has changed
        if ((termsAndConditions.IsCurrentVersion && !existingTerms.IsCurrentVersion) ||
            (termsAndConditions.IsCurrentVersion && termsAndConditions.DocumentType != existingTerms.DocumentType))
        {
            await SetDocumentTypeAsNotCurrentAsync(termsAndConditions.DocumentType);
        }

        return await _repository.UpdateTermsAndConditionsAsync(termsAndConditions);
    }

    public async Task<bool> DeleteTermsAndConditionsAsync(string id)
    {
        return await _repository.DeleteTermsAndConditionsAsync(id);
    }

    public async Task<TermsAndConditions> MakeTermsAndConditionsCurrentAsync(string id)
    {
        var terms = await _repository.GetTermsAndConditionsByIdAsync(id);
        if (terms == null)
        {
            return null;
        }

        // Only continue if the terms is not already current
        if (!terms.IsCurrentVersion)
        {
            // Set all terms of the same document type to not current
            await SetDocumentTypeAsNotCurrentAsync(terms.DocumentType);

            // Set the specified terms as current
            terms.IsCurrentVersion = true;
            await _repository.UpdateTermsAndConditionsAsync(terms);
        }

        return terms;
    }

    private async Task SetDocumentTypeAsNotCurrentAsync(string documentType)
    {
        var currentTerms = await _repository.GetCurrentTermsAndConditionsAsync(documentType);
        if (currentTerms != null)
        {
            currentTerms.IsCurrentVersion = false;
            await _repository.UpdateTermsAndConditionsAsync(currentTerms);
        }
    }
}