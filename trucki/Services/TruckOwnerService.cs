using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class TruckOwnerService: ITruckOwnerService
{
    private readonly ITruckOwnerRepository _ownerRepository;
    public TruckOwnerService(ITruckOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;

    }
    


    public async Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model)
    {
        var res = await _ownerRepository.CreateNewTruckOwner(model);
        return res;
    }

    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id)
    {
        var res = await _ownerRepository.GetTruckOwnerById(id);
        return res;
    }

    public async Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model)
    {
        var res = await _ownerRepository.EditTruckOwner(model);
        return res;
    }

    public async Task<ApiResponseModel<bool>> DeleteTruckOwner(string id)
    {
        var res = await _ownerRepository.DeleteTruckOwner(id);
        return res;
    }

    public async Task<ApiResponseModel<List<AllTruckOwnerResponseModel>>> GetAllTruckOwners()
    {
        var res = await _ownerRepository.GetAllTruckOwners();
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> SearchTruckOwners(string searchWords)
    {
        var res = await _ownerRepository.SearchTruckOwners(searchWords);
        return res;
    }
      public async Task<ApiResponseModel<bool>> AddNewTransporter(AddTransporterRequestBody model)
    {
        var res = await _ownerRepository.AddNewTransporter(model);
        return res;
    }
    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetTransporterProfileById(string id)
    {
        var res = await _ownerRepository.GetTransporterProfileById(id);
        return res;
    }
     // New methods for managing TruckOwner status
    public async Task<ApiResponseModel<bool>> ApproveTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.ApproveTruckOwner(truckOwnerId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> NotApproveTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.NotApproveTruckOwner(truckOwnerId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> BlockTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.BlockTruckOwner(truckOwnerId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> UnblockTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.UnblockTruckOwner(truckOwnerId);
        return res;
    }

     public async Task<ApiResponseModel<bool>> UploadIdCardAndProfilePicture(string truckOwnerId, string idCardUrl, string profilePictureUrl)
    {
        var res = await _ownerRepository.UploadIdCardAndProfilePicture(truckOwnerId, idCardUrl, profilePictureUrl);
        return res;
    }

    public async Task<ApiResponseModel<bool>> UpdateBankDetails(UpdateBankDetailsRequestBody model)
    {
        var res = await _ownerRepository.UpdateBankDetails(model.Id, model);
        return res;
    }

}