using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface ITruckOwnerRepository
{
    Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id);
    Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model);
    Task<ApiResponseModel<bool>> DeleteTruckOwner(string id);
    Task<ApiResponseModel<List<TruckOwnerResponseModel>>> GetAllTruckOwners();
    Task<ApiResponseModel<IEnumerable<TruckOwnerResponseModel>>> SearchTruckOwners(string searchWords);

}