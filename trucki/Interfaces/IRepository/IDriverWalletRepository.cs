using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository
{
    public interface IDriverWalletRepository
    {
        // Get wallet balance and details for a driver
        Task<ApiResponseModel<DriverWalletBalanceResponseModel>> GetWalletBalanceAsync(string driverId);
        
        // Credit driver's wallet after delivery completion
        Task<ApiResponseModel<bool>> CreditDeliveryAmountAsync(
            string driverId, 
            string orderId,
            decimal amount, 
            string description);
        
        // Process weekly automatic withdrawals
        Task<ApiResponseModel<DriverWithdrawalResultModel>> ProcessWeeklyWithdrawalsAsync(string adminId);
        
        // Calculate amounts for upcoming withdrawal cycles
        Task<ApiResponseModel<bool>> UpdateWithdrawalProjectionsAsync();
        
        // Get transaction history with pagination and filtering
        Task<ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>> GetTransactionHistoryAsync(
            string driverId, 
            GetDriverWalletTransactionsQueryDto query);
        
        // Get withdrawal schedule overview - for both driver and admin
        Task<ApiResponseModel<DriverWithdrawalScheduleResponseModel>> GetWithdrawalScheduleAsync(string driverId);
        
        // Admin function to get all pending withdrawals
        Task<ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>> GetAllPendingWithdrawalsAsync();
        
        // Ensure a wallet exists for the driver (create if not)
        Task<DriverWallet> EnsureWalletExistsAsync(string driverId);
        
        // Get driver earnings summary
        Task<ApiResponseModel<DriverEarningsSummaryResponseModel>> GetEarningsSummaryAsync(
            string driverId,
            DateTime startDate,
            DateTime endDate);
    }
}