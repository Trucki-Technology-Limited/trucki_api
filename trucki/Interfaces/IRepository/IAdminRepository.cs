using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface IAdminRepository
{
    Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel request);
    Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness();
    Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model);
    Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id);
}