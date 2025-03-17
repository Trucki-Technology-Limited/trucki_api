using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface ITruckRepository
{
    Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model);
    Task<ApiResponseModel<string>> AddDriverOwnedTruck(DriverAddTruckRequestModel model);
    Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model);
    Task<ApiResponseModel<string>> DeleteTruck(string truckId);
    Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId);
    Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords);
    Task<ApiResponseModel<List<AllTruckResponseModel>>> GetAllTrucks();
    Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId);
    Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model);
    Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model);
    Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByOwnersId(string ownersId);
    Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByDriverId(string driverId);
    Task<ApiResponseModel<TruckStatusCountResponseModel>> GetTruckStatusCountByOwnerId(string ownerId);
    Task<ApiResponseModel<string>> UpdateApprovalStatusAsync(string truckId, ApprovalStatus approvalStatus);
    Task<ApiResponseModel<bool>> UpdateTruckPhotos(UpdateTruckPhotosRequestModel model);
}