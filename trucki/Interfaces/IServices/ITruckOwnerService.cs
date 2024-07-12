using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface ITruckOwnerService
{
    Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id);
    Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model);
    Task<ApiResponseModel<bool>> DeleteTruckOwner(string id);
    Task<ApiResponseModel<List<AllTruckOwnerResponseModel>>> GetAllTruckOwners();
    Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> SearchTruckOwners(string searchWords);
}