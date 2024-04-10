using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IAdminService
{
    Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel request);
    Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness();
    Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model);
    Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id);
    Task<ApiResponseModel<bool>> EditBusiness(EditBusinessRequestModel model);
    Task<ApiResponseModel<bool>> DeleteBusiness(string id);
    Task<ApiResponseModel<bool>> DisableBusiness(string id);
    Task<ApiResponseModel<bool>> EnableBusiness(string id);
    Task<ApiResponseModel<bool>> EditRoute(EditRouteRequestModel model);
    Task<ApiResponseModel<bool>> DeleteRoute(string id);
    Task<ApiResponseModel<string>> AddManager(AddManagerRequestModel model);
    Task<ApiResponseModel<bool>> EditManager(EditManagerRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateDriver(string driverId);
    Task<ApiResponseModel<bool>> DeactivateManager(string managerId);
    Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model);
    Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model);
    Task<ApiResponseModel<List<AllManagerResponseModel>>> GetAllManager();
    Task<ApiResponseModel<AllManagerResponseModel>> GetManagerById(string id);
    Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id);
    Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model);
    Task<ApiResponseModel<bool>> DeleteTruckOwner(string id);
    Task<ApiResponseModel<List<TruckOwnerResponseModel>>> GetAllTruckOwners();
    Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers();
    Task<ApiResponseModel<AllDriverResponseModel>> GetDriverById(string id);
    Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords);
    Task<ApiResponseModel<IEnumerable<AllManagerResponseModel>>> SearchManagers(string searchWords);
    Task<ApiResponseModel<IEnumerable<AllBusinessResponseModel>>> SearchBusinesses(string searchWords);
    Task<ApiResponseModel<string>> AddOfficer(AddOfficerRequestModel model);
    Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllFieldOfficers(int page, int size);
    Task<ApiResponseModel<bool>> EditOfficer(EditOfficerRequestModel model);
    Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model);
    Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model);
    Task<ApiResponseModel<string>> DeleteTruck(string truckId);
    Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId);
    Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords);

}