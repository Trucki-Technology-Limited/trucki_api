using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }


    public async Task<ApiResponseModel<DashboardSummaryResponse>> GetDashBoardData()
    {
        var res = await _adminRepository.GetDashBoardData();
        return res;
    }

    public async Task<ApiResponseModel<GtvDashboardSummary>> GetGtvDashBoardSummary(DateTime startDate,
        DateTime endDate)
    {
        var res = await _adminRepository.GetGtvDashBoardSummary(startDate, endDate);
        return res;
    }

    public async Task<ApiResponseModel<TruckDahsBoardData>> GetTruckDashboardData(string truckId)
    {
        var res = await _adminRepository.GetTruckDashboardData(truckId);
        return res;
    }
    public async Task<ApiResponseModel<OrderStatsResponse>> GetOrderStatistics(DateTime startDate,
        DateTime endDate)
    {
        var res = await _adminRepository.GetOrderStatistics(startDate, endDate);
        return res;
    }
    
    public async Task<ApiResponseModel<CargoFinancialSummaryResponse>> GetCargoFinancialSummary(DateTime startDate, DateTime endDate)
    {
        var response = await _adminRepository.GetCargoFinancialSummary(startDate, endDate);
        return response;
    }

}