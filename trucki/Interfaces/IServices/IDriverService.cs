using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IDriverService
{
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers();
    Task<DriverResponseModel> GetDriverById(string id);
    Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords);
    Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model);
    Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateDriver(string driverId);
    Task<ApiResponseModel<DriverProfileResponseModel>> GetDriverProfileById(string driverId);
    Task<ApiResponseModel<OrderCountByDriver>> GetOrderCountByDriver(string driverId);
    Task<ApiResponseModel<List<AllOrderResponseModel>>> GetOrderAssignedToDriver(string driverId);
    Task<ApiResponseModel<string>> CreateDriverAccount(CreateDriverRequestModel model);
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetDriversByTruckOwnerId(string truckOwnerId);
    Task<ApiResponseModel<bool>> AcceptTermsAndConditions(AcceptTermsRequestModel model);
    Task<ApiResponseModel<bool>> HasAcceptedLatestTerms(string driverId);
    Task<ApiResponseModel<List<TermsAcceptanceRecordDto>>> GetTermsAcceptanceHistory(string driverId);
    Task<ApiResponseModel<bool>> UpdateDriverProfilePhoto(UpdateDriverProfilePhotoRequestModel model);
    Task<ApiResponseModel<bool>> UpdateDriverOnboardingStatus(string driverId, DriverOnboardingStatus status);
}