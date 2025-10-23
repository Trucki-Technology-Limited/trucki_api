using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IDriverService
{
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers();
    Task<DriverResponseModel> GetDriverById(string id, string requesterId = null, string requesterRole = null);
    Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords);
    Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model);
    Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateDriver(string driverId);
    Task<ApiResponseModel<DriverProfileResponseModel>> GetDriverProfileById(string driverId);
    Task<ApiResponseModel<OrderCountByDriver>> GetOrderCountByDriver(string driverId);
    Task<ApiResponseModel<List<AllOrderResponseModel>>> GetOrderAssignedToDriver(string driverId);
    Task<ApiResponseModel<string>> CreateDriverAccount(CreateDriverRequestModel model);
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetDriversForFleetManagerById(string fleetManagerId);
    Task<ApiResponseModel<string>> AddDriverToFleetManager(AddDriverToFleetManagerRequestModel model);
    Task<ApiResponseModel<bool>> AcceptTermsAndConditions(AcceptTermsRequestModel model);
    Task<ApiResponseModel<bool>> HasAcceptedLatestTerms(string driverId);
    Task<ApiResponseModel<List<TermsAcceptanceRecordDto>>> GetTermsAcceptanceHistory(string driverId);
    Task<ApiResponseModel<bool>> UpdateDriverProfilePhoto(UpdateDriverProfilePhotoRequestModel model);
    Task<ApiResponseModel<bool>> UpdateDriverOnboardingStatus(string driverId, DriverOnboardingStatus status);
    Task<ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>> GetAllDriversPaginated(GetAllDriversRequestModel request);
    Task<ApiResponseModel<AdminDriverSummaryResponseModel>> GetAdminDriversSummary();
    Task<ApiResponseModel<bool>> UpdateDotNumber(UpdateDotNumberRequestModel model);
    Task<ApiResponseModel<bool>> UpdateTransportationNumbers(UpdateTransportationNumbersRequestModel model);
    Task<ApiResponseModel<AdminDriverDetailsResponseModel>> GetDriverDetailsForAdmin(string driverId);

    // Complete driver approval process
    Task<ApiResponseModel<bool>> CompleteDriverApprovalAsync(string driverId);
}