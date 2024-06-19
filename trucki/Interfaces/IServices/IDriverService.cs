using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IDriverService
{
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers();
    Task<ApiResponseModel<AllDriverResponseModel>> GetDriverById(string id);
    Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords);
    Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model);
    Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateDriver(string driverId);
}