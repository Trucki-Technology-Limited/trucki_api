using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface ITruckService
{
    Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model);
    Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model);
    Task<ApiResponseModel<string>> DeleteTruck(string truckId);
    Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId);
    Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords);
    Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> GetAllTrucks();
    Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId);
    Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model);
    Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model);
}