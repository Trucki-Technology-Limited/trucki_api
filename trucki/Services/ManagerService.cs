using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class ManagerService : IManagerService
{
    private readonly IManagerRepository _managerRepository;
    public ManagerService(IManagerRepository managerRepository)
    {
        _managerRepository = managerRepository;

    }
    public async Task<ApiResponseModel<string>> AddManager(AddManagerRequestModel model)
    {
        var res = await _managerRepository.AddManager(model);
        return res;
    }

    public async Task<ApiResponseModel<List<AllManagerResponseModel>>> GetAllManager()
    {
        var responseModel = await _managerRepository.GetAllManager();
        if (responseModel.IsSuccessful)
        {
            return new ApiResponseModel<List<AllManagerResponseModel>>
            {
                IsSuccessful = responseModel.IsSuccessful,
                Message = responseModel.Message,
                StatusCode = responseModel.StatusCode,
                Data = responseModel.Data
            };
        }
        return new ApiResponseModel<List<AllManagerResponseModel>> { IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode };
    }

    public async Task<ApiResponseModel<AllManagerResponseModel>> GetManagerById(string id)
    {
        var responseModel = await _managerRepository.GetManagerById(id);
        if (responseModel.IsSuccessful)
        {
            return new ApiResponseModel<AllManagerResponseModel> { IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode, Data = responseModel.Data };
        }
        return new ApiResponseModel<AllManagerResponseModel> { IsSuccessful = responseModel.IsSuccessful, Message = responseModel.Message, StatusCode = responseModel.StatusCode };

    }

    public async Task<ApiResponseModel<bool>> DeactivateManager(string managerId)
    {
        var res = await _managerRepository.DeactivateManager(managerId);
        return res;
    }
    public async Task<ApiResponseModel<bool>> EditManager(EditManagerRequestModel model)
    {
        var res = await _managerRepository.EditManager(model);
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllManagerResponseModel>>> SearchManagers(string searchWords)
    {
        var res = await _managerRepository.SearchManagers(searchWords);
        return res;
    }
    public async Task<string> GetManagerIdAsync(string? userId)
    {
        var res = await _managerRepository.GetManagerIdAsync(userId);
        return res;
    }
    public async Task<ApiResponseModel<ManagerDashboardData>> GetManagerDashboardData(List<string> userRoles,
string userId)
    {
        var res = await _managerRepository.GetManagerDashboardData(userRoles, userId);
        return res;
    }

    public async Task<ApiResponseModel<List<TransactionResponseModel>>> GetTransactionsByManager(string userId)
    {
        var res = await _managerRepository.GetTransactionsByManager(userId);
        return res;
    }
    public async Task<ApiResponseModel<TransactionSummaryResponseModel>> GetTransactionSummaryResponseModel(string userId)
    {
        var res = await _managerRepository.GetTransactionSummaryResponseModel(userId);
        return res;
    }
    public async Task<ApiResponseModel<List<TransactionResponseModel>>> GetTransactionsByFinancialManager(string userId)
    {
        var res = await _managerRepository.GetTransactionsByFinancialManager(userId);
        return res;
    }
    public async Task<ApiResponseModel<TransactionSummaryResponseModel>> GetFinancialTransactionSummaryResponseModel(string userId)
    {
        var res = await _managerRepository.GetFinancialTransactionSummaryResponseModel(userId);
        return res;
    }
    public async Task<ApiResponseModel<GtvDashboardSummary>> GetManagerGtvDashBoardSummary(DateTime startDate, DateTime endDate, List<string> userRoles,
string userId)
    {
        var res = await _managerRepository.GetManagerGtvDashBoardSummary(startDate, endDate, userRoles, userId);
        return res;
    }
     public async Task<ApiResponseModel<string>> EditAssignedBusinesses(EditAssignedBusinessesRequestModel model)
    {
        return await _managerRepository.EditAssignedBusinesses(model);
    }
}