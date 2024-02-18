using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IAdminService
{
    Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel request);
    Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness();
}