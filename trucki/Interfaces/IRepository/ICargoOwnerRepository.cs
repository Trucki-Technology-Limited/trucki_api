using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface ICargoOwnerRepository
{
    Task<ApiResponseModel<string>> CreateCargoOwnerAccount(CreateCargoOwnerRequestModel model);
    Task<ApiResponseModel<bool>> EditCargoOwnerProfile(EditCargoOwnerRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateCargoOwner(string cargoOwnerId);
    Task<ApiResponseModel<List<CargoOrderResponseModel>>> GetCargoOwnerOrders(GetCargoOwnerOrdersQueryDto query);
    Task<ApiResponseModel<CargoOwnerProfileResponseModel>> GetCargoOwnerProfile(string userId);
}