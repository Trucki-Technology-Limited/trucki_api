using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface IAdminRepository
{
    Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel request);
    Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness();
    Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model);
    Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id);
    Task<ApiResponseModel<bool>> EditBusiness(EditBusinessRequestModel model);
    Task<ApiResponseModel<bool>> DeleteBusiness(string id);
    Task<ApiResponseModel<bool>> DisableBusiness(string id);
    Task<ApiResponseModel<bool>> EnableBusiness(string id);
    Task<ApiResponseModel<bool>> EditRoute(EditRouteRequestModel model);
    Task<ApiResponseModel<bool>> DeleteRoute(string id);
    Task<ApiResponseModel<string>> AddManager(AddManagerRequestModel model);
    Task<ApiResponseModel<bool>> EditManager(EditManagerRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateManager(string managerId);
    Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model);
    Task<ApiResponseModel<List<AllManagerResponseModel>>> GetAllManager();
    Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id);
    Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model);
    Task<ApiResponseModel<bool>> DeleteTruckOwner(string id);
    Task<ApiResponseModel<List<TruckOwnerResponseModel>>> GetAllTruckOwners();
}