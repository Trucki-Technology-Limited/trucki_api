using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repository;

public class TermsAndConditionsRepository : ITermsAndConditionsRepository
{
    private readonly TruckiDBContext _context;

    public TermsAndConditionsRepository(TruckiDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TermsAndConditions>> GetAllTermsAndConditionsAsync()
    {
        return await _context.TermsAndConditions
            .OrderByDescending(t => t.IsCurrentVersion)
            .ThenBy(t => t.DocumentType)
            .ThenByDescending(t => t.EffectiveDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TermsAndConditions>> GetTermsAndConditionsByDocumentTypeAsync(string documentType)
    {
        return await _context.TermsAndConditions
            .Where(t => t.DocumentType == documentType)
            .OrderByDescending(t => t.IsCurrentVersion)
            .ThenByDescending(t => t.EffectiveDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TermsAndConditions> GetTermsAndConditionsByIdAsync(string id)
    {
        return await _context.TermsAndConditions
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TermsAndConditions> GetCurrentTermsAndConditionsAsync(string documentType)
    {
        return await _context.TermsAndConditions
            .Where(t => t.IsCurrentVersion && t.DocumentType == documentType)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TermsAndConditions>> GetAllCurrentTermsAndConditionsAsync()
    {
        return await _context.TermsAndConditions
            .Where(t => t.IsCurrentVersion)
            .OrderBy(t => t.DocumentType)
            .ToListAsync();
    }

    public async Task<TermsAndConditions> CreateTermsAndConditionsAsync(TermsAndConditions termsAndConditions)
    {
        await _context.TermsAndConditions.AddAsync(termsAndConditions);
        await _context.SaveChangesAsync();
        return termsAndConditions;
    }

    public async Task<TermsAndConditions> UpdateTermsAndConditionsAsync(TermsAndConditions termsAndConditions)
    {
        _context.TermsAndConditions.Update(termsAndConditions);
        await _context.SaveChangesAsync();
        return termsAndConditions;
    }

    public async Task<bool> DeleteTermsAndConditionsAsync(string id)
    {
        var termsToDelete = await _context.TermsAndConditions
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (termsToDelete == null)
        {
            return false;
        }

        // Don't allow deleting the current version
        if (termsToDelete.IsCurrentVersion)
        {
            throw new InvalidOperationException("Cannot delete the current version of Terms and Conditions.");
        }

        _context.TermsAndConditions.Remove(termsToDelete);
        await _context.SaveChangesAsync();
        return true;
    }
}