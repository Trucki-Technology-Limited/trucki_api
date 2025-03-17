using trucki.Entities;

namespace trucki.Interfaces.IServices;

public interface ITermsAndConditionsService
{
    Task<IEnumerable<TermsAndConditions>> GetAllTermsAndConditionsAsync();
    Task<IEnumerable<TermsAndConditions>> GetTermsAndConditionsByDocumentTypeAsync(string documentType);
    Task<TermsAndConditions> GetTermsAndConditionsByIdAsync(string id);
    Task<TermsAndConditions> GetCurrentTermsAndConditionsAsync(string documentType);
    Task<IEnumerable<TermsAndConditions>> GetAllCurrentTermsAndConditionsAsync();
    Task<TermsAndConditions> CreateTermsAndConditionsAsync(TermsAndConditions termsAndConditions);
    Task<TermsAndConditions> UpdateTermsAndConditionsAsync(TermsAndConditions termsAndConditions);
    Task<bool> DeleteTermsAndConditionsAsync(string id);
    Task<TermsAndConditions> MakeTermsAndConditionsCurrentAsync(string id);
}