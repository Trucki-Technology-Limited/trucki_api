using trucki.Entities;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IDriverDocumentService
    {
        Task<DriverDocument> GetDriverDocumentAsync(string driverDocumentId);
        Task<IEnumerable<DriverDocument>> GetDriverDocumentsAsync(string driverId);

        Task<DriverDocument> CreateDriverDocumentAsync(DriverDocument driverDocument);
        Task<DriverDocument> ApproveDocumentAsync(string driverDocumentId);
        Task<DriverDocument> RejectDocumentAsync(string driverDocumentId, string reason);

        Task<bool> DeleteDriverDocumentAsync(string driverDocumentId);

        // For retrieving "required" documents that the driver hasn't uploaded or is missing
        Task<IEnumerable<DocumentType>> GetRequiredDocumentTypesForDriverAsync(string driverId);
        Task<bool> AreAllRequiredDocumentsApprovedAsync(string driverId);
        Task<IEnumerable<DriverDocumentStatusDto>> GetDriverDocumentSummaryAsync(string driverId);
    }
}
