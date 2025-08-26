using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface IAdminRepository
{
    Task<ApiResponseModel<DashboardSummaryResponse>> GetDashBoardData();
    Task<ApiResponseModel<GtvDashboardSummary>> GetGtvDashBoardSummary(DateTime startDate, DateTime endDate);
    Task<ApiResponseModel<TruckDahsBoardData>> GetTruckDashboardData(string truckId);
    Task<ApiResponseModel<OrderStatsResponse>> GetOrderStatistics(DateTime startDate, DateTime endDate);
    Task<ApiResponseModel<CargoFinancialSummaryResponse>> GetCargoFinancialSummary(DateTime startDate, DateTime endDate);
}