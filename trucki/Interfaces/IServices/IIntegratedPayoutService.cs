using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IIntegratedPayoutService
{
    Task<ApiResponseModel<IntegratedPayoutSummaryModel>> ProcessWeeklyPayoutsAsync(string processedBy);
    Task<ApiResponseModel<bool>> ProcessOrderCompletionAsync(string orderId, decimal driverEarnings);
}