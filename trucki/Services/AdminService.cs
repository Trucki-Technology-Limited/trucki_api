using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;

    }
    public async Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel model)
    {
        var createBusiness = await _adminRepository.CreateNewBusiness(model);
        if (createBusiness.IsSuccessful)
        {
            return new ApiResponseModel<bool> { IsSuccessful = true, Message = "Business created Successfully", StatusCode = 201, Data = true };
        }
        return new ApiResponseModel<bool> { IsSuccessful = createBusiness.IsSuccessful, Message = createBusiness.Message, StatusCode = createBusiness.StatusCode };
    }
    public async Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness()
    {
        var getBusinesses = await _adminRepository.GetAllBusiness();
        if (getBusinesses.IsSuccessful)
        {
            return new ApiResponseModel<List<AllBusinessResponseModel>>
            {
                IsSuccessful = getBusinesses.IsSuccessful, Message = getBusinesses.Message, StatusCode = getBusinesses.StatusCode,
                Data = getBusinesses.Data
            };
        }
        return new ApiResponseModel<List<AllBusinessResponseModel>> { IsSuccessful = getBusinesses.IsSuccessful, Message = getBusinesses.Message, StatusCode = getBusinesses.StatusCode };
    }
    public async Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model)
    {
        var addRoute = await _adminRepository.AddRouteToBusiness(model);
        if (addRoute.IsSuccessful)
        {
            return new ApiResponseModel<bool> { IsSuccessful = true, Message = "Route created Successfully", StatusCode = 201, Data = true };
        }
        return new ApiResponseModel<bool> { IsSuccessful = addRoute.IsSuccessful, Message = addRoute.Message, StatusCode = addRoute.StatusCode };
    }
    public async Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id)
    {
        var getBusiness = await _adminRepository.GetBusinessById(id);
        if (getBusiness.IsSuccessful)
        {
            return new ApiResponseModel<BusinessResponseModel>
            {
                IsSuccessful = getBusiness.IsSuccessful, Message = getBusiness.Message, StatusCode = getBusiness.StatusCode,
                Data = getBusiness.Data
            };
        }
        return new ApiResponseModel<BusinessResponseModel> { IsSuccessful = getBusiness.IsSuccessful, Message = getBusiness.Message, StatusCode = getBusiness.StatusCode };
    }

    public async Task<ApiResponseModel<bool>> EditBusiness(EditBusinessRequestModel model)
    {
        var editBusiness = await _adminRepository.EditBusiness(model);
        return editBusiness;
    }

    public async Task<ApiResponseModel<bool>> DeleteBusiness(string id)
    {
        var deleteBusiness = await _adminRepository.DeleteBusiness(id);
        return deleteBusiness;
    }

    public async Task<ApiResponseModel<bool>> DisableBusiness(string id)
    {
        var disableBusiness = await _adminRepository.DisableBusiness(id);
        return disableBusiness;
    }
    public async Task<ApiResponseModel<bool>> EnableBusiness(string id)
    {
        var enableBusiness = await _adminRepository.EnableBusiness(id);
        return enableBusiness;
    }

    public async Task<ApiResponseModel<bool>> EditRoute(EditRouteRequestModel model)
    {
        var editRoute = await _adminRepository.EditRoute(model);
        return editRoute;
    }

    public async Task<ApiResponseModel<bool>> DeleteRoute(string id)
    {
        var deleteRoute = await _adminRepository.DeleteRoute(id);
        return deleteRoute;
    }
    public async Task<ApiResponseModel<string>> AddManager(AddManagerRequestModel model)
    {
        var res = await _adminRepository.AddManager(model);
        return res;
    }
    public async Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model)
    {
        var res = await _adminRepository.AddDriver(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model)
    {
        var res = await _adminRepository.EditDriver(model);
        return res;
    }
    public async Task<ApiResponseModel<List<AllManagerResponseModel>>> GetAllManager()
    {
        var responseModel = await _adminRepository.GetAllManager();
        if (responseModel.IsSuccessful)
        {
            return new ApiResponseModel<List<AllManagerResponseModel>>
            {
                IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode,
                Data = responseModel.Data
            };
        }
        return new ApiResponseModel<List<AllManagerResponseModel>> { IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode };
    }

    public async Task<ApiResponseModel<AllManagerResponseModel>> GetManagerById(string id)
    {
        var responseModel = await _adminRepository.GetManagerById(id);
        if (responseModel.IsSuccessful)
        {
            return new ApiResponseModel<AllManagerResponseModel> { IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode, Data = responseModel.Data };
        }
        return new ApiResponseModel<AllManagerResponseModel> { IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode };

    }

    public async Task<ApiResponseModel<bool>> DeactivateManager(string managerId)
    {
        var res = await _adminRepository.DeactivateManager(managerId);
        return res;
    }
    public async Task<ApiResponseModel<bool>> EditManager(EditManagerRequestModel model)
    {
        var res = await _adminRepository.EditManager(model);
        return res;
    }

    public async Task<ApiResponseModel<bool>> DeactivateDriver(string driverId)
    {
        var res = await _adminRepository.DeactivateDriver(driverId);
        return res;
    }


    public async Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model)
    {
        var res = await _adminRepository.CreateNewTruckOwner(model);
        return res;
    }

    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id)
    {
        var res = await _adminRepository.GetTruckOwnerById(id);
        return res;
    }

    public async Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model)
    {
        var res = await _adminRepository.EditTruckOwner(model);
        return res;
    }

    public async Task<ApiResponseModel<bool>> DeleteTruckOwner(string id)
    {
        var res = await _adminRepository.DeleteTruckOwner(id);
        return res;
    }

    public async Task<ApiResponseModel<List<TruckOwnerResponseModel>>> GetAllTruckOwners()
    {
        var res = await _adminRepository.GetAllTruckOwners();
        return res;
    }
    public async Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers()
    {
        var res = await _adminRepository.GetAllDrivers();
        return res;
    }

    public async Task<ApiResponseModel<AllDriverResponseModel>> GetDriverById(string id)
    {
        var res = await _adminRepository.GetDriverById(id);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords)
    {
        var res = await _adminRepository.SearchDrivers(searchWords);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllManagerResponseModel>>> SearchManagers(string searchWords)
    {
        var res = await _adminRepository.SearchManagers(searchWords);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllBusinessResponseModel>>> SearchBusinesses(string searchWords)
    {
        var res = await _adminRepository.SearchBusinesses(searchWords);
        return res;
    }
    public async Task<ApiResponseModel<string>> AddOfficer(AddOfficerRequestModel model)
    {
        var res = await _adminRepository.AddOfficer(model);
        return res;
    }
    public async Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllFieldOfficers(int page, int size)
    {
        var res = await _adminRepository.GetAllFieldOfficers(page, size);
        return res;
    }

    public async Task<ApiResponseModel<bool>> EditOfficer(EditOfficerRequestModel model)
    {
        var res = await _adminRepository.EditOfficer(model);
        return res;
    }
    public async Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model)
    {
        var res = await _adminRepository.AddNewTruck(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model)
    {
        var res = await _adminRepository.EditTruck(model);
        return res;
    }
    public async Task<ApiResponseModel<string>> DeleteTruck(string truckId)
    {
        var res = await _adminRepository.DeleteTruck(truckId);
        return res;
    }
    public async Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId)
    {
        var res = await _adminRepository.GetTruckById(truckId);
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords)
    {
        var res = await _adminRepository.SearchTruck(searchWords);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> GetAllTrucks()
    {
        var res = await _adminRepository.GetAllTrucks();
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId)
    {
        var res = await _adminRepository.GetTruckDocuments(truckId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
    {
        var res = await _adminRepository.AssignDriverToTruck(model);
        return res;
    }

    public async Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
    {
        var res = await _adminRepository.UpdateTruckStatus(truckId, model);
        return res;
    }




}