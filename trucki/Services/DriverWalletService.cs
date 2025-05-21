using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class DriverWalletService : IDriverWalletService
    {
        private readonly IDriverWalletRepository _driverWalletRepository;
        private readonly ILogger<DriverWalletService> _logger;

        public DriverWalletService(
            IDriverWalletRepository driverWalletRepository,
            ILogger<DriverWalletService> logger)
        {
            _driverWalletRepository = driverWalletRepository;
            _logger = logger;
        }

        public async Task<ApiResponseModel<DriverWalletBalanceResponseModel>> GetWalletBalanceAsync(string driverId)
        {
            return await _driverWalletRepository.GetWalletBalanceAsync(driverId);
        }

        public async Task<ApiResponseModel<bool>> CreditDeliveryAmountAsync(
            string driverId,
            string orderId,
            decimal amount,
            string description)
        {
            return await _driverWalletRepository.CreditDeliveryAmountAsync(driverId, orderId, amount, description);
        }

        public async Task<ApiResponseModel<DriverWithdrawalResultModel>> ProcessWeeklyWithdrawalsAsync(string adminId)
        {
            return await _driverWalletRepository.ProcessWeeklyWithdrawalsAsync(adminId);
        }

        public async Task<ApiResponseModel<bool>> UpdateWithdrawalProjectionsAsync()
        {
            return await _driverWalletRepository.UpdateWithdrawalProjectionsAsync();
        }

        public async Task<ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>> GetTransactionHistoryAsync(
            string driverId,
            GetDriverWalletTransactionsQueryDto query)
        {
            return await _driverWalletRepository.GetTransactionHistoryAsync(driverId, query);
        }

        public async Task<ApiResponseModel<DriverWithdrawalScheduleResponseModel>> GetWithdrawalScheduleAsync(string driverId)
        {
            return await _driverWalletRepository.GetWithdrawalScheduleAsync(driverId);
        }

        public async Task<ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>> GetAllPendingWithdrawalsAsync()
        {
            return await _driverWalletRepository.GetAllPendingWithdrawalsAsync();
        }

        public async Task<DriverWallet> EnsureWalletExistsAsync(string driverId)
        {
            return await _driverWalletRepository.EnsureWalletExistsAsync(driverId);
        }

        public async Task<ApiResponseModel<DriverEarningsSummaryResponseModel>> GetEarningsSummaryAsync(
            string driverId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _driverWalletRepository.GetEarningsSummaryAsync(driverId, startDate, endDate);
        }
    }
}