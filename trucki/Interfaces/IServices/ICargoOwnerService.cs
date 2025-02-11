using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;



namespace trucki.Interfaces.IServices;
public interface ICargoOwnerService
{
    Task<ApiResponseModel<string>> CreateCargoOwnerAccount(CreateCargoOwnerRequestModel model);
    Task<ApiResponseModel<bool>> EditCargoOwnerProfile(EditCargoOwnerRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateCargoOwner(string cargoOwnerId);
    Task<ApiResponseModel<List<CargoOrderResponseModel>>> GetCargoOwnerOrders(GetCargoOwnerOrdersQueryDto query);
    Task<ApiResponseModel<CargoOwnerProfileResponseModel>> GetCargoOwnerProfile(string userId);
}