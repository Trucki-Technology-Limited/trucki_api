using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IBusinessService
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
    Task<ApiResponseModel<IEnumerable<AllBusinessResponseModel>>> SearchBusinesses(string searchWords);
    Task<ApiResponseModel<List<RouteResponseModel>>> GetRoutesByBusinessId(string businessId);
}