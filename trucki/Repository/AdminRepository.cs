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


    public async Task<ApiResponseModel<GtvDashboardSummary>> GetGtvDashBoardSummary(DateTime startDate, DateTime endDate)
    {
        // validate startDate and endDates
        if (startDate > endDate)
        {
            return new ApiResponseModel<GtvDashboardSummary>
            {
                Data = null,
                IsSuccessful = false,
                Message = "Start date must be less than or equal to end date",
                StatusCode = 400
            };
        }

        // Get orders within the date range
        List<Order> orders = await _context.Orders.Include(e => e.Routes)
            .Where(o => o.OrderStatus == OrderStatus.Delivered)
            .Where(o => o.StartDate >= startDate && o.EndDate <= endDate)
            .ToListAsync();

        // Initialize dictionary to hold monthly data
        Dictionary<string, (float income, float revenue, float payout)> monthlyData = new Dictionary<string, (float, float, float)>();

        // Generate all months between the startDate and endDate
        DateTime currentDate = new DateTime(startDate.Year, startDate.Month, 1);
        DateTime endMonth = new DateTime(endDate.Year, endDate.Month, 1);

        while (currentDate <= endMonth)
        {
            string monthName = currentDate.ToString("MMM");
            monthlyData[monthName] = (0, 0, 0); // Initialize with zeros for income, revenue, and payout
            currentDate = currentDate.AddMonths(1); // Move to the next month
        }

        // Process orders and group by month
        foreach (var order in orders)
        {
            string monthName = order.StartDate.ToString("MMM"); // Get short month name

            if (monthlyData.ContainsKey(monthName))
            {
                float orderGtv = order.Routes?.Gtv ?? 0;
                float orderRevenue = order.Routes?.Price ?? 0;

                monthlyData[monthName] = (
                    income: monthlyData[monthName].income + orderGtv,
                    revenue: orderGtv - monthlyData[monthName].revenue + orderRevenue,
                    payout: monthlyData[monthName].payout + orderRevenue
                );
            }
        }

        // Convert dictionary to list for chart data, ensuring it's in the correct order
        var chartData = monthlyData.Select(m => new LineChartEntry
        {
            Name = m.Key,
            Income = m.Value.income,
            Revenue = m.Value.revenue,
            Payout = m.Value.payout
        }).ToList();

        // Calculate total GTV, revenue, and payout
        var totalGtv = orders.Sum(o => o.Routes != null ? o.Routes.Gtv : 0);
        var totalPrice = orders.Sum(o => o.Routes != null ? o.Routes.Price : 0);
        var totalIncome = orders.Sum(o => (o.Routes != null ? o.Routes.Gtv : 0) - (o.Routes != null ? o.Routes.Price : 0));

        var summary = new GtvDashboardSummary
        {
            TotalGtv = totalGtv,
            TotalRevenue = totalIncome,
            TotalPayout = totalPrice,
            MonthlyData = chartData
        };

        return new ApiResponseModel<GtvDashboardSummary>
        {
            Data = summary,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200
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

    public async Task<ApiResponseModel<OrderStatsResponse>> GetOrderStatistics(DateTime startDate, DateTime endDate)
    {
        // Validate date range
        if (startDate > endDate)
        {
            return new ApiResponseModel<OrderStatsResponse>
            {
                Data = null,
                IsSuccessful = false,
                Message = "Start date must be less than or equal to end date",
                StatusCode = 400
            };
        }

        // Filter orders within the date range
        var orders = await _context.Orders
            .Where(o => o.StartDate >= startDate && o.EndDate <= endDate)
            .ToListAsync();

        // Initialize dictionary to hold monthly data
        Dictionary<string, (int completed, int flagged, int inTransit, int total)> monthlyData = new Dictionary<string, (int, int, int, int)>();

        // Generate all months between the startDate and endDate
        DateTime currentDate = new DateTime(startDate.Year, startDate.Month, 1);
        DateTime endMonth = new DateTime(endDate.Year, endDate.Month, 1);

        while (currentDate <= endMonth)
        {
            string monthName = currentDate.ToString("MMM yyyy"); // Use "MMM yyyy" for year inclusion
            monthlyData[monthName] = (0, 0, 0, 0); // Initialize with zeros
            currentDate = currentDate.AddMonths(1);
        }

        // Process orders and group by month
        foreach (var order in orders)
        {
            string monthName = order.StartDate.ToString("MMM yyyy");

            if (monthlyData.ContainsKey(monthName))
            {
                int completedCount = order.OrderStatus == OrderStatus.Delivered ? 1 : 0;
                int flaggedCount = order.OrderStatus == OrderStatus.Flagged ? 1 : 0;
                int inTransitCount = order.OrderStatus == OrderStatus.InTransit ? 1 : 0;

                monthlyData[monthName] = (
                    completed: monthlyData[monthName].completed + completedCount,
                    flagged: monthlyData[monthName].flagged + flaggedCount,
                    inTransit: monthlyData[monthName].inTransit + inTransitCount,
                    total: monthlyData[monthName].total + 1 // Total orders
                );
            }
        }

        // Convert dictionary to list for chart data, ensuring it's in the correct order
        var monthlyStats = monthlyData.Select(m => new MonthlyOrderStats
        {
            Month = m.Key,
            CompletedOrders = m.Value.completed,
            FlaggedOrders = m.Value.flagged,
            InTransitOrders = m.Value.inTransit,
            TotalOrders = m.Value.total
        }).ToList();

        // Calculate total stats for the entire range
        var totalCompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
        var totalFlaggedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Flagged);
        var totalInTransitOrders = orders.Count(o => o.OrderStatus == OrderStatus.InTransit);
        var totalOrders = orders.Count();

        var stats = new OrderStatsResponse
        {
            CompletedOrders = totalCompletedOrders,
            FlaggedOrders = totalFlaggedOrders,
            InTransitOrders = totalInTransitOrders,
            TotalOrders = totalOrders,
            MonthlyData = monthlyStats // Add monthly data to response
        };

        return new ApiResponseModel<OrderStatsResponse>
        {
            Data = stats,
            IsSuccessful = true,
            Message = "Order statistics by month",
            StatusCode = 200
        };
    }

}