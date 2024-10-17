using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface IDriverRepository
{
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers();
    Task<ApiResponseModel<DriverResponseModel>> GetDriverById(string id);
    Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords);
    Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model);
    Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateDriver(string driverId);
    Task<ApiResponseModel<DriverProfileResponseModel>> GetDriverProfileById(string driverId);
    Task<ApiResponseModel<OrderCountByDriver>> GetOrderCountByDriver(string driverId);
    Task<ApiResponseModel<List<AllOrderResponseModel>>> GetOrderAssignedToDriver(string driverId);
    Task<ApiResponseModel<string>> CreateDriverAccount(CreateDriverRequestModel model);
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetDriversByTruckOwnerId(string truckOwnerId);
}