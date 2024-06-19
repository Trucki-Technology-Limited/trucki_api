using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IManagerService
{
    Task<ApiResponseModel<IEnumerable<AllManagerResponseModel>>> SearchManagers(string searchWords);
    Task<ApiResponseModel<string>> AddManager(AddManagerRequestModel model);
    Task<ApiResponseModel<bool>> EditManager(EditManagerRequestModel model);
    Task<ApiResponseModel<bool>> DeactivateManager(string managerId);
    Task<ApiResponseModel<List<AllManagerResponseModel>>> GetAllManager();
    Task<string> GetManagerIdAsync(string? userId);
    Task<ApiResponseModel<ManagerDashboardData>> GetManagerDashboardData(string managerId);
    Task<ApiResponseModel<AllManagerResponseModel>> GetManagerById(string id);
    Task<ApiResponseModel<List<TransactionResponseModel>>> GetTransactionsByManager(string userId);
    Task<ApiResponseModel<TransactionSummaryResponseModel>> GetTransactionSummaryResponseModel(string userId);
    Task<ApiResponseModel<List<TransactionResponseModel>>> GetTransactionsByFinancialManager(string userId);
    Task<ApiResponseModel<TransactionSummaryResponseModel>> GetFinancialTransactionSummaryResponseModel(string userId);
}