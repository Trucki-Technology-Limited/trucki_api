using trucki.Entities;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IDriverDocumentService
    {
        Task<DriverDocument> GetDriverDocumentAsync(string driverDocumentId);
        Task<IEnumerable<DriverDocument>> GetDriverDocumentsAsync(string driverId);

        Task<DriverDocument> CreateDriverDocumentAsync(DriverDocument driverDocument);
        Task<DriverDocumentResponseDto> ApproveDocumentAsync(string driverDocumentId);
        Task<DriverDocumentResponseDto> RejectDocumentAsync(string driverDocumentId, string reason);

        Task<bool> DeleteDriverDocumentAsync(string driverDocumentId);

        // For retrieving "required" documents that the driver hasn't uploaded or is missing
        Task<IEnumerable<DocumentType>> GetRequiredDocumentTypesForDriverAsync(string driverId);
        Task<bool> AreAllRequiredDocumentsApprovedAsync(string driverId);
        Task<IEnumerable<DriverDocumentStatusDto>> GetDriverDocumentSummaryAsync(string driverId);

        // Batch operations for admin
        Task<ApiResponseModel<BatchApprovalResponseModel>> BatchApproveDocumentsAsync(List<string> documentIds);
        Task<ApiResponseModel<BatchApprovalResponseModel>> BatchRejectDocumentsAsync(List<string> documentIds, string rejectionReason);
    }
}
