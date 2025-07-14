using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository
{
    public interface ITruckRepository
    {
        // Existing methods (keeping for backward compatibility)
        Task<ApiResponseModel<List<AllTruckResponseModel>>> GetAllTrucks();
        Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId);
        Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model);
        Task<ApiResponseModel<string>> AddDriverOwnedTruck(DriverAddTruckRequestModel model);
        Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model);
        Task<ApiResponseModel<string>> DeleteTruck(string truckId);
        Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords);
        Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId);
        Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model);
        Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model);
        Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByOwnersId(string ownersId);
        Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByDriverId(string driverId);
        Task<ApiResponseModel<TruckStatusCountResponseModel>> GetTruckStatusCountByOwnerId(string ownerId);
        Task<ApiResponseModel<string>> UpdateApprovalStatusAsync(string truckId, ApprovalStatus approvalStatus);
        Task<ApiResponseModel<bool>> UpdateTruckPhotos(UpdateTruckPhotosRequestModel model);

        // New enhanced methods
        /// <summary>
        /// Get all trucks with enhanced filtering, pagination, and search
        /// </summary>
        Task<ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>> GetAllTrucksEnhancedAsync(GetTrucksQueryDto query);
        
        /// <summary>
        /// Get comprehensive truck status and approval status counts
        /// </summary>
        Task<ApiResponseModel<EnhancedTruckStatusCountResponseModel>> GetTruckStatusCountsEnhancedAsync();
        
        /// <summary>
        /// Get detailed truck information by ID with driver details and trip statistics
        /// </summary>
        Task<ApiResponseModel<TruckDetailResponseModel>> GetTruckByIdEnhancedAsync(string truckId);
        
        /// <summary>
        /// Export trucks as CSV based on query filters
        /// </summary>
        Task<ApiResponseModel<byte[]>> ExportTrucksAsCsvAsync(GetTrucksQueryDto query);
        
        /// <summary>
        /// Update truck approval status with enhanced functionality
        /// </summary>
        Task<ApiResponseModel<string>> UpdateApprovalStatusEnhancedAsync(string truckId, UpdateApprovalStatusRequest request);
        
        /// <summary>
        /// Get truck status counts by owner with enhanced details
        /// </summary>
        Task<ApiResponseModel<EnhancedTruckStatusCountResponseModel>> GetTruckStatusCountsByOwnerIdAsync(string ownerId);
    }
}