using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class AdminService: IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
         
    }
    public async Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel model)
    {
        var createBusiness = await _adminRepository.CreateNewBusiness(model);
        if (createBusiness.IsSuccessful)
        {
            return new ApiResponseModel<bool> { IsSuccessful = true, Message = "Business created Successfully", StatusCode = 201, Data = true };
        }
        return new ApiResponseModel<bool> { IsSuccessful = createBusiness.IsSuccessful, Message = createBusiness.Message, StatusCode = createBusiness.StatusCode };
    }
    public async Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness()
    {
        var getBusinesses = await _adminRepository.GetAllBusiness();
        if (getBusinesses.IsSuccessful)
        {
            return new ApiResponseModel<List<AllBusinessResponseModel>>
            {
                IsSuccessful = getBusinesses.IsSuccessful, Message = getBusinesses.Message, StatusCode = getBusinesses.StatusCode,
                Data = getBusinesses.Data
            };
        }
        return new ApiResponseModel<List<AllBusinessResponseModel>> { IsSuccessful = getBusinesses.IsSuccessful, Message = getBusinesses.Message, StatusCode = getBusinesses.StatusCode };
    }
    public async Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model)
    {
        var addRoute = await _adminRepository.AddRouteToBusiness(model);
        if (addRoute.IsSuccessful)
        {
            return new ApiResponseModel<bool> { IsSuccessful = true, Message = "Route created Successfully", StatusCode = 201, Data = true };
        }
        return new ApiResponseModel<bool> { IsSuccessful = addRoute.IsSuccessful, Message = addRoute.Message, StatusCode = addRoute.StatusCode };
    }
    public async Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id)
    {
        var getBusiness = await _adminRepository.GetBusinessById(id);
        if (getBusiness.IsSuccessful)
        {
            return new ApiResponseModel<BusinessResponseModel>
            {
                IsSuccessful = getBusiness.IsSuccessful, Message = getBusiness.Message, StatusCode = getBusiness.StatusCode,
                Data = getBusiness.Data
            };
        }
        return new ApiResponseModel<BusinessResponseModel> { IsSuccessful = getBusiness.IsSuccessful, Message = getBusiness.Message, StatusCode = getBusiness.StatusCode };
    }
}