using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IAccountDeletionService
    {
        Task<ApiResponseModel<bool>> RequestAccountDeletion(string userId, AccountDeletionRequestModel model);
        Task<ApiResponseModel<AccountDeletionResponseModel>> GetAccountDeletionRequest(string userId);
    }
}