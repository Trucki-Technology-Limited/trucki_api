using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IDriverPayoutService
{
    Task<ApiResponseModel<PayoutSummaryResponseModel>> ProcessWeeklyPayoutsAsync(string processedBy);
    Task<ApiResponseModel<DriverPayoutResponseModel>> ProcessDriverPayoutAsync(string driverId, string processedBy, bool forceProcess = false);
    Task<ApiResponseModel<List<DriverPayoutResponseModel>>> GetDriverPayoutHistoryAsync(string driverId, int page = 1, int pageSize = 10);
    Task<ApiResponseModel<DriverEarningsProjectionModel>> GetDriverEarningsProjectionAsync(string driverId);
    Task<ApiResponseModel<List<DriverEarningsProjectionModel>>> GetAllDriverEarningsProjectionsAsync();
    Task<ApiResponseModel<bool>> RecalculateDriverEarningsAsync(string orderId);
}