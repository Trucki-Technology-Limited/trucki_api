using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;


public interface IStripeConnectService
{
    Task<ApiResponseModel<StripeConnectAccountResponseModel>> CreateConnectAccountAsync(CreateStripeConnectAccountRequestModel request);
    Task<ApiResponseModel<bool>> UpdateAccountStatusAsync(UpdateStripeAccountStatusRequestModel request);
    Task<ApiResponseModel<string>> RefreshAccountLinkAsync(string driverId);
    Task<ApiResponseModel<bool>> TransferToDriverAsync(string driverId, decimal amount, string description, string? orderId = null);
    Task<ApiResponseModel<bool>> VerifyAccountCanReceivePayoutsAsync(string stripeAccountId);
    Task<ApiResponseModel<StripeConnectAccountResponseModel>> GetDriverStripeInfoAsync(string driverId);
}
