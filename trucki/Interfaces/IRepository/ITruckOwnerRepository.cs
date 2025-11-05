using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface ITruckOwnerRepository
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
    Task<ApiResponseModel<bool>> UpdateBankDetails(string truckOwnerId, UpdateBankDetailsRequestBody model);

    // New methods for getting specific owner types with filtering and sorting
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> GetDispatchers(string? searchTerm = null, string? sortBy = "date");
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> GetTruckOwners(string? searchTerm = null, string? sortBy = "date");
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> GetTransporters(string? searchTerm = null, string? sortBy = "date");
}