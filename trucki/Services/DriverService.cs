using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class DriverService: IDriverService
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

    public async Task<ApiResponseModel<DriverResponseModel>> GetDriverById(string id)
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
}