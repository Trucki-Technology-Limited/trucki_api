
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;
public interface IDriverBankAccountService
{
    Task<ApiResponseModel<DriverBankAccountResponseDto>> AddBankAccountAsync(AddDriverBankAccountDto request);
    Task<ApiResponseModel<List<DriverBankAccountResponseDto>>> GetDriverBankAccountsAsync(string driverId);
    Task<ApiResponseModel<bool>> SetDefaultBankAccountAsync(string driverId, string accountId);
    Task<ApiResponseModel<bool>> DeleteBankAccountAsync(string driverId, string accountId);
    Task<ApiResponseModel<bool>> VerifyBankAccountAsync(string accountId, string adminId);
}