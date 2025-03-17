using trucki.Entities;

namespace trucki.Interfaces.IRepository;

public interface ITermsAndConditionsRepository
{
    Task<IEnumerable<TermsAndConditions>> GetAllTermsAndConditionsAsync();
    Task<IEnumerable<TermsAndConditions>> GetTermsAndConditionsByDocumentTypeAsync(string documentType);
    Task<TermsAndConditions> GetTermsAndConditionsByIdAsync(string id);
    Task<TermsAndConditions> GetCurrentTermsAndConditionsAsync(string documentType);
    Task<IEnumerable<TermsAndConditions>> GetAllCurrentTermsAndConditionsAsync();
    Task<TermsAndConditions> CreateTermsAndConditionsAsync(TermsAndConditions termsAndConditions);
    Task<TermsAndConditions> UpdateTermsAndConditionsAsync(TermsAndConditions termsAndConditions);
    Task<bool> DeleteTermsAndConditionsAsync(string id);
}
