using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class DriverDocumentService : IDriverDocumentService
    {
        private readonly IDriverDocumentRepository _driverDocumentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IDriverRepository _driverRepository;
        // ^ or however you fetch the Driver
        // so you can get the driver's Country, etc.

        public DriverDocumentService(
            IDriverDocumentRepository driverDocumentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IDriverRepository driverRepository)
        {
            _driverDocumentRepository = driverDocumentRepository;
            _documentTypeRepository = documentTypeRepository;
            _driverRepository = driverRepository;
        }

        public async Task<DriverDocument> GetDriverDocumentAsync(string driverDocumentId)
        {
            return await _driverDocumentRepository.GetByIdAsync(driverDocumentId);
        }

        public async Task<IEnumerable<DriverDocument>> GetDriverDocumentsAsync(string driverId)
        {
            return await _driverDocumentRepository.GetAllForDriverAsync(driverId);
        }

        public async Task<DriverDocument> CreateDriverDocumentAsync(DriverDocument driverDocument)
        {
            // Possibly do validation, e.g. ensure driverDocument.DocumentTypeId is valid
            // Ensure file is uploaded, etc.
            driverDocument.SubmittedAt = DateTime.UtcNow;
            driverDocument.ApprovalStatus = "Pending";
            return await _driverDocumentRepository.CreateAsync(driverDocument);
        }

        public async Task<DriverDocument> ApproveDocumentAsync(string driverDocumentId)
        {
            var doc = await _driverDocumentRepository.GetByIdAsync(driverDocumentId);
            if (doc == null) return null;

            doc.ApprovalStatus = "Approved";
            doc.ReviewedAt = DateTime.UtcNow;
            doc.RejectionReason = null; // Clear any existing reason
            return await _driverDocumentRepository.UpdateAsync(doc);
        }

        public async Task<DriverDocument> RejectDocumentAsync(string driverDocumentId, string reason)
        {
            var doc = await _driverDocumentRepository.GetByIdAsync(driverDocumentId);
            if (doc == null) return null;

            doc.ApprovalStatus = "Rejected";
            doc.ReviewedAt = DateTime.UtcNow;
            doc.RejectionReason = reason;
            return await _driverDocumentRepository.UpdateAsync(doc);
        }

        public async Task<bool> DeleteDriverDocumentAsync(string driverDocumentId)
        {
            return await _driverDocumentRepository.DeleteAsync(driverDocumentId);
        }

        // Get a list of DocumentType that are REQUIRED for the driver's country/entity
        public async Task<IEnumerable<DocumentType>> GetRequiredDocumentTypesForDriverAsync(string driverId)
        {
            var driver = await _driverRepository.GetDriverById(driverId);
            if (driver == null)
            {
                return Enumerable.Empty<DocumentType>();
            }

            // Example: if your DocumentType has fields like: Country, EntityType = "Driver"
            return await _documentTypeRepository.GetAllRequiredForCountryAndEntityAsync(
                driver.Country,
                "Driver" // or driver.EntityType, if stored
            );
        }

        // Check if the driver has uploaded (and had approved) all required docs
        public async Task<bool> AreAllRequiredDocumentsApprovedAsync(string driverId)
        {
            var requiredDocs = await GetRequiredDocumentTypesForDriverAsync(driverId);
            if (!requiredDocs.Any()) return true; // maybe no required docs

            var driverDocs = await GetDriverDocumentsAsync(driverId);

            // For each required DocumentType, ensure there's at least one Approved doc
            foreach (var requiredType in requiredDocs)
            {
                var matchingDoc = driverDocs
                    .FirstOrDefault(dd => dd.DocumentTypeId == requiredType.Id);

                // if no matching doc or it's not approved, fail
                if (matchingDoc == null || matchingDoc.ApprovalStatus != "Approved")
                {
                    return false;
                }
            }

            return true;
        }
         public async Task<IEnumerable<DriverDocumentStatusDto>> GetDriverDocumentSummaryAsync(string driverId)
        {
            // 1. Get the driver
            var driver = await _driverRepository.GetDriverById(driverId);
            if (driver == null)
            {
                // Return empty or throw an exception that the controller catches
                return Enumerable.Empty<DriverDocumentStatusDto>();
            }

            // 2. Get required DocumentTypes for this driver (country, entityType = "Driver")
            var requiredTypes = await _documentTypeRepository
                .GetAllRequiredForCountryAndEntityAsync(driver.Country, "Driver");

            // 3. Get the existing documents the driver has uploaded
            var driverDocs = await _driverDocumentRepository.GetAllForDriverAsync(driverId);

            // 4. Build a status list
            var result = new List<DriverDocumentStatusDto>();

            foreach (var docType in requiredTypes)
            {
                // Find a matching driver document for this docType (if any)
                var matchingDoc = driverDocs
                    .FirstOrDefault(d => d.DocumentTypeId == docType.Id);

                // If no matching doc, it's "NotUploaded"
                if (matchingDoc == null)
                {
                    result.Add(new DriverDocumentStatusDto
                    {
                        DocumentTypeId = docType.Id,
                        DocumentTypeName = docType.Name,
                        IsRequired = docType.IsRequired,
                        IsUploaded = false,
                        ApprovalStatus = "NotUploaded",
                        FileUrl = null,
                        RejectionReason = null
                    });
                }
                else
                {
                    result.Add(new DriverDocumentStatusDto
                    {
                        DocumentTypeId = docType.Id,
                        DocumentTypeName = docType.Name,
                        IsRequired = docType.IsRequired,
                        IsUploaded = true,
                        ApprovalStatus = matchingDoc.ApprovalStatus,  // "Pending", "Approved", or "Rejected"
                        FileUrl = matchingDoc.FileUrl,
                        RejectionReason = matchingDoc.RejectionReason
                    });
                }
            }

            // If you also want to consider 'optional' doc types or doc types that are
            // not strictly required, you could add logic for them too.
            // For now, we only included the "required" doc types.

            return result;
        }
    }
}
