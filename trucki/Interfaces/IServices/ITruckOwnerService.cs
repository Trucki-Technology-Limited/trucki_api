using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface ITruckOwnerService
{
    Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id);
    Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model);
    Task<ApiResponseModel<bool>> DeleteTruckOwner(string id);
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> SearchTruckOwners(string searchWords);
    Task<ApiResponseModel<bool>> AddNewTransporter(AddTransporterRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetTransporterProfileById(string transporterId);
    Task<ApiResponseModel<bool>> ApproveTruckOwner(string truckOwnerId);
    Task<ApiResponseModel<bool>> NotApproveTruckOwner(string truckOwnerId);
    Task<ApiResponseModel<bool>> BlockTruckOwner(string truckOwnerId);
    Task<ApiResponseModel<bool>> UnblockTruckOwner(string truckOwnerId);
    Task<ApiResponseModel<bool>> UploadIdCardAndProfilePicture(string truckOwnerId, string idCardUrl, string profilePictureUrl);
    Task<ApiResponseModel<bool>> UpdateBankDetails(UpdateBankDetailsRequestBody model);

    // UNIFIED FLEET OWNER METHODS (Dispatcher + Transporter)
    Task<ApiResponseModel<bool>> RegisterFleetOwner(AddDispatcherRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetFleetOwnerProfile(string userId);

    // Dispatcher-specific methods
    Task<ApiResponseModel<List<DriverResponseModel>>> GetDispatcherDrivers(string dispatcherId);

    // Driver onboarding methods for dispatchers
    Task<ApiResponseModel<string>> AddDriverToDispatcher(AddDriverForDispatcherRequestModel model);
    Task<ApiResponseModel<bool>> UploadDriverDocuments(UploadDriverDocumentsForDispatcherDto model);
    Task<ApiResponseModel<bool>> AddTruckForDriver(AddTruckForDispatcherDriverDto model);
    Task<ApiResponseModel<bool>> CompleteDriverOnboarding(CompleteDriverOnboardingDto model);

    // Commission management
    Task<ApiResponseModel<bool>> SetOrUpdateDriverCommission(string driverId, string dispatcherId, decimal commissionPercentage);
    Task<ApiResponseModel<DriverCommissionHistoryResponseModel>> GetDriverCommissionHistory(string driverId, string dispatcherId);

    // DOT and MC number management
    Task<ApiResponseModel<bool>> UpdateDispatcherDotMcNumbers(UpdateDispatcherDotMcNumbersRequestModel model);

    // New methods for getting specific owner types with filtering and sorting
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> GetDispatchers(string? searchTerm = null, string? sortBy = "date");
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> GetTruckOwners(string? searchTerm = null, string? sortBy = "date");
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> GetTransporters(string? searchTerm = null, string? sortBy = "date");
}