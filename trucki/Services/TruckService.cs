using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class TruckService: ITruckService
{
    private readonly ITruckRepository _truckRepository;
    public TruckService(ITruckRepository truckRepository)
    {
        _truckRepository = truckRepository;

    }
    
 
  
    public async Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model)
    {
        var res = await _truckRepository.AddNewTruck(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model)
    {
        var res = await _truckRepository.EditTruck(model);
        return res;
    }
    public async Task<ApiResponseModel<string>> DeleteTruck(string truckId)
    {
        var res = await _truckRepository.DeleteTruck(truckId);
        return res;
    }
    public async Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId)
    {
        var res = await _truckRepository.GetTruckById(truckId);
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords)
    {
        var res = await _truckRepository.SearchTruck(searchWords);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> GetAllTrucks()
    {
        var res = await _truckRepository.GetAllTrucks();
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId)
    {
        var res = await _truckRepository.GetTruckDocuments(truckId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
    {
        var res = await _truckRepository.AssignDriverToTruck(model);
        return res;
    }

    public async Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
    {
        var res = await _truckRepository.UpdateTruckStatus(truckId, model);
        return res;
    }
    public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByOwnersId(string ownersId)
    {
        var res = await _truckRepository.GetTrucksByOwnersId(ownersId);
        return res;
    }
}