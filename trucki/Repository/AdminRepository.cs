using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using Microsoft.AspNetCore.Identity;
using trucki.CustomExtension;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class AdminRepository : IAdminRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public AdminRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
    }
    


    public async Task<ApiResponseModel<DashboardSummaryResponse>> GetDashBoardData()
    {
        int totalBusiness = await _context.Businesses.CountAsync();
        int totalCustomers = await _context.Customers.CountAsync();

        int totalTrucks = await _context.Trucks.CountAsync();

        int totalActiveTrucks = await _context.Trucks.CountAsync(t =>
            t.TruckStatus == TruckStatus.EnRoute || t.TruckStatus == TruckStatus.Available);

        var dashboardSummary = new DashboardSummaryResponse
        {
            TotalBusiness = totalBusiness,
            TotalCustomers = totalCustomers,
            TotalTrucks = totalTrucks,
            TotalActiveTrucks = totalActiveTrucks
        };

        return new ApiResponseModel<DashboardSummaryResponse>
        {
            Data = dashboardSummary,
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Successful"
        };
    }

 

    public async Task<ApiResponseModel<GtvDashboardSummary>> GetGtvDashBoardSummary(DateTime startDate,
        DateTime endDate)
    {
        // validate satrtDate and endDate
        if (startDate > endDate)
        {
            return new ApiResponseModel<GtvDashboardSummary>
            {
                Data = null,
                IsSuccessful = false,
                Message = "Start date must be less than or equal to end datae",
                StatusCode = 400
            };
        }

        List<Order> orders = await _context.Orders.Where(o => o.StartDate >= startDate && o.EndDate <= endDate)
            .ToListAsync();

        float totalGtv = orders.Sum(o => o.Routes?.Gtv ?? 0);
        float totalRevenue = orders.Sum(o => o.Routes?.Price ?? 0);

        var summary = new GtvDashboardSummary
        {
            TotalGtv = totalGtv,
            TotalRevenue = totalRevenue
        };

        return new ApiResponseModel<GtvDashboardSummary>
        {
            Data = summary,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<TruckDahsBoardData>> GetTruckDashboardData(string truckId)
    {
        var orders = await _context.Orders
            .Include(o => o.Truck)
            .Where(o => o.TruckId == truckId)
            .ToListAsync();

        int completedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
        int flaggedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Flagged);
        decimal totalOrderPrice = orders.Sum(o => decimal.Parse(o.Routes?.Price.ToString() ?? "0"));


        var stats = new TruckDahsBoardData
        {
            CompletedOrders = completedOrders,
            FlaggedOrders = flaggedOrders,
            TotalOrderPrice = totalOrderPrice
        };

        return new ApiResponseModel<TruckDahsBoardData>
            { Data = stats, IsSuccessful = true, Message = "Dashboard data", StatusCode = 200 };
    }
    
    public async Task<ApiResponseModel<OrderStatsResponse>> GetOrderStatistics()
    {
        // Total Completed Orders
        var completedOrders = await _context.Orders
            .CountAsync(o => o.OrderStatus == OrderStatus.Delivered);

        // Total Flagged Orders
        var flaggedOrders = await _context.Orders
            .CountAsync(o => o.OrderStatus == OrderStatus.Flagged);

        // Total Orders in Transit
        var inTransitOrders = await _context.Orders
            .CountAsync(o => o.OrderStatus == OrderStatus.InTransit);

        //Total Number of Orders
        var totalOrder = await _context.Orders
            .CountAsync();

        var stats = new OrderStatsResponse
        {
            CompletedOrders = completedOrders,
            FlaggedOrders = flaggedOrders,
            InTransitOrders = inTransitOrders,
            TotalOrders = totalOrder
        };

        return new ApiResponseModel<OrderStatsResponse>
        {
            Data = stats,
            IsSuccessful = true,
            Message = "Order statistics",
            StatusCode = 200
        };
    }
    
}