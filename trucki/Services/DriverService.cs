using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;
    public DriverService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;

    }
    public async Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model)
    {
        var res = await _driverRepository.AddDriver(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model)
    {
        var res = await _driverRepository.EditDriver(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> DeactivateDriver(string driverId)
    {
        var res = await _driverRepository.DeactivateDriver(driverId);
        return res;
    }
    public async Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers()
    {
        var res = await _driverRepository.GetAllDrivers();
        return res;
    }

    public async Task<DriverResponseModel> GetDriverById(string id)
    {
        var res = await _driverRepository.GetDriverById(id);
        return res;
    }
    public async Task<ApiResponseModel<DriverProfileResponseModel>> GetDriverProfileById(string id)
    {
        var res = await _driverRepository.GetDriverProfileById(id);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords)
    {
        var res = await _driverRepository.SearchDrivers(searchWords);
        return res;
    }
    public async Task<ApiResponseModel<OrderCountByDriver>> GetOrderCountByDriver(string searchWords)
    {
        var res = await _driverRepository.GetOrderCountByDriver(searchWords);
        return res;
    }
    public async Task<ApiResponseModel<List<AllOrderResponseModel>>> GetOrderAssignedToDriver(string id)
    {
        var res = await _driverRepository.GetOrderAssignedToDriver(id);
        return res;
    }

    public async Task<ApiResponseModel<string>> CreateDriverAccount(CreateDriverRequestModel model)
    {
        var res = await _driverRepository.CreateDriverAccount(model);
        return res;
    }
    public async Task<ApiResponseModel<List<AllDriverResponseModel>>> GetDriversByTruckOwnerId(string truckOwnerId)
    {
        var res = await _driverRepository.GetDriversByTruckOwnerId(truckOwnerId);
        return res;
    }
    public async Task<ApiResponseModel<bool>> AcceptTermsAndConditions(AcceptTermsRequestModel model)
    {
        var res = await _driverRepository.AcceptTermsAndConditions(model);
        return res;
    }

    public async Task<ApiResponseModel<bool>> HasAcceptedLatestTerms(string driverId)
    {
        var res = await _driverRepository.HasAcceptedLatestTerms(driverId);
        return res;
    }

    public async Task<ApiResponseModel<List<TermsAcceptanceRecordDto>>> GetTermsAcceptanceHistory(string driverId)
    {
        var res = await _driverRepository.GetTermsAcceptanceHistory(driverId);
        return res;
    }
    public async Task<ApiResponseModel<bool>> UpdateDriverProfilePhoto(UpdateDriverProfilePhotoRequestModel model)
    {
        var res = await _driverRepository.UpdateDriverProfilePhoto(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> UpdateDriverOnboardingStatus(string driverId, DriverOnboardingStatus status)
    {
        var res = await _driverRepository.UpdateDriverOnboardingStatus(driverId, status);
        return res;
    }
    public async Task<ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>> GetAllDriversPaginated(GetAllDriversRequestModel request)
    {
        var response = await _driverRepository.GetAllDriversPaginated(request);
        return response;
    }

    public async Task<ApiResponseModel<AdminDriverSummaryResponseModel>> GetAdminDriversSummary()
    {
        var response = await _driverRepository.GetAdminDriversSummary();
        return response;
    }

    public async Task<ApiResponseModel<bool>> UpdateDotNumber(UpdateDotNumberRequestModel model)
    {
        var response = await _driverRepository.UpdateDotNumber(model);
        return response;
    }

    public async Task<ApiResponseModel<AdminDriverDetailsResponseModel>> GetDriverDetailsForAdmin(string driverId)
    {
        var response = await _driverRepository.GetDriverDetailsForAdmin(driverId);
        return response;
    }

    public async Task<ApiResponseModel<bool>> CompleteDriverApprovalAsync(string driverId)
    {
        var response = await _driverRepository.CompleteDriverApprovalAsync(driverId);
        return response;
    }
}