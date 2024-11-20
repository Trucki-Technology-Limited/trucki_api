using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;

    }
    public async Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel model)
    {
        var createBusiness = await _businessRepository.CreateNewBusiness(model);
        if (createBusiness.IsSuccessful)
        {
            return new ApiResponseModel<bool> { IsSuccessful = true, Message = "Business created Successfully", StatusCode = 201, Data = true };
        }
        return new ApiResponseModel<bool> { IsSuccessful = createBusiness.IsSuccessful, Message = createBusiness.Message, StatusCode = createBusiness.StatusCode };
    }
    public async Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness()
    {
        var getBusinesses = await _businessRepository.GetAllBusiness();
        if (getBusinesses.IsSuccessful)
        {
            return new ApiResponseModel<List<AllBusinessResponseModel>>
            {
                IsSuccessful = getBusinesses.IsSuccessful,
                Message = getBusinesses.Message,
                StatusCode = getBusinesses.StatusCode,
                Data = getBusinesses.Data
            };
        }
        return new ApiResponseModel<List<AllBusinessResponseModel>> { IsSuccessful = getBusinesses.IsSuccessful, Message = getBusinesses.Message, StatusCode = getBusinesses.StatusCode };
    }
    public async Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model)
    {
        var addRoute = await _businessRepository.AddRouteToBusiness(model);
        if (addRoute.IsSuccessful)
        {
            return new ApiResponseModel<bool> { IsSuccessful = true, Message = "Route created Successfully", StatusCode = 201, Data = true };
        }
        return new ApiResponseModel<bool> { IsSuccessful = addRoute.IsSuccessful, Message = addRoute.Message, StatusCode = addRoute.StatusCode };
    }
    public async Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id)
    {
        var getBusiness = await _businessRepository.GetBusinessById(id);
        if (getBusiness.IsSuccessful)
        {
            return new ApiResponseModel<BusinessResponseModel>
            {
                IsSuccessful = getBusiness.IsSuccessful,
                Message = getBusiness.Message,
                StatusCode = getBusiness.StatusCode,
                Data = getBusiness.Data
            };
        }
        return new ApiResponseModel<BusinessResponseModel> { IsSuccessful = getBusiness.IsSuccessful, Message = getBusiness.Message, StatusCode = getBusiness.StatusCode };
    }

    public async Task<ApiResponseModel<bool>> EditBusiness(EditBusinessRequestModel model)
    {
        var editBusiness = await _businessRepository.EditBusiness(model);
        return editBusiness;
    }

    public async Task<ApiResponseModel<bool>> DeleteBusiness(string id)
    {
        var deleteBusiness = await _businessRepository.DeleteBusiness(id);
        return deleteBusiness;
    }

    public async Task<ApiResponseModel<bool>> DisableBusiness(string id)
    {
        var disableBusiness = await _businessRepository.DisableBusiness(id);
        return disableBusiness;
    }
    public async Task<ApiResponseModel<bool>> EnableBusiness(string id)
    {
        var enableBusiness = await _businessRepository.EnableBusiness(id);
        return enableBusiness;
    }

    public async Task<ApiResponseModel<bool>> EditRoute(EditRouteRequestModel model)
    {
        var editRoute = await _businessRepository.EditRoute(model);
        return editRoute;
    }

    public async Task<ApiResponseModel<bool>> DeleteRoute(string id)
    {
        var deleteRoute = await _businessRepository.DeleteRoute(id);
        return deleteRoute;
    }
    public async Task<ApiResponseModel<IEnumerable<AllBusinessResponseModel>>> SearchBusinesses(string searchWords)
    {
        var res = await _businessRepository.SearchBusinesses(searchWords);
        return res;
    }
    public async Task<ApiResponseModel<List<RouteResponseModel>>> GetRoutesByBusinessId(string businessId)
    {
        var res = await _businessRepository.GetRoutesByBusinessId(businessId);
        return res;
    }
    public async Task<ApiResponseModel<BusinessGtvDashboardSummary>> GetBusinessGtvDashboardSummary(DateTime startDate, DateTime endDate, string businessId)
    {
        var res = await _businessRepository.GetBusinessGtvDashboardSummary(startDate, endDate, businessId);
        return res;
    }

}