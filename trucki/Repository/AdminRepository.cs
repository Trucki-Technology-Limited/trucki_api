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
    // Existing counts
    int totalBusiness = await _context.Businesses.CountAsync();
    int totalCustomers = await _context.Customers.CountAsync();
    int totalTrucks = await _context.Trucks.CountAsync();
    int totalActiveTrucks = await _context.Trucks.CountAsync(t =>
        t.TruckStatus == TruckStatus.EnRoute || t.TruckStatus == TruckStatus.Available);

    // New counts you requested
    int totalManagers = await _context.Managers.CountAsync(m => m.IsActive);
    
    int totalFieldOfficers = await _context.Officers.CountAsync(o => 
        o.IsActive && o.OfficerType == OfficerType.FieldOfficer);
    
    int totalTruckOwners = await _context.TruckOwners.CountAsync();
    
    int totalOrdersCompleted = await _context.Orders.CountAsync(o => 
        o.OrderStatus == OrderStatus.Delivered);
    
    int totalCargoOrdersCompleted = await _context.CargoOrders.CountAsync(co => 
        co.Status == CargoOrderStatus.Delivered);
    
    // Count drivers by country - assuming you have a Country field in Driver entity
    int totalNigerianDrivers = await _context.Drivers.CountAsync(d => 
        d.IsActive && (d.Country == "Nigeria" || d.Country == "NG"));
    
    int totalUSDrivers = await _context.Drivers.CountAsync(d => 
        d.IsActive && (d.Country == "United States" || d.Country == "US" || d.Country == "USA"));
    
    int totalCargoOwners = await _context.CargoOwners.CountAsync();

    var dashboardSummary = new DashboardSummaryResponse
    {
        TotalBusiness = totalBusiness,
        TotalCustomers = totalCustomers,
        TotalTrucks = totalTrucks,
        TotalActiveTrucks = totalActiveTrucks,
        TotalManagers = totalManagers,
        TotalFieldOfficers = totalFieldOfficers,
        TotalTruckOwners = totalTruckOwners,
        TotalOrdersCompleted = totalOrdersCompleted,
        TotalCargoOrdersCompleted = totalCargoOrdersCompleted,
        TotalNigerianDrivers = totalNigerianDrivers,
        TotalUSDrivers = totalUSDrivers,
        TotalCargoOwners = totalCargoOwners
    };

    return new ApiResponseModel<DashboardSummaryResponse>
    {
        Data = dashboardSummary,
        IsSuccessful = true,
        StatusCode = 200,
        Message = "Dashboard data retrieved successfully"
    };
}


public async Task<ApiResponseModel<GtvDashboardSummary>> GetGtvDashBoardSummary(DateTime startDate, DateTime endDate)
{
    // Validate date range
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

    try
    {
        // Single optimized query - only fetch required fields
        var orderData = await _context.Orders
            .Where(o => o.OrderStatus == OrderStatus.Delivered)
            .Where(o => o.StartDate >= startDate && o.EndDate <= endDate)
            .Where(o => o.Routes != null) // Ensure route exists
            .Select(o => new
            {
                StartDate = o.StartDate,
                Gtv = o.Routes.Gtv,
                Price = o.Routes.Price
            })
            .ToListAsync();

        // Initialize monthly data structure
        var monthlyData = InitializeMonthlyGtvData(startDate, endDate);

        // Single pass calculation for totals and monthly data
        float totalGtv = 0;
        float totalPrice = 0;

        foreach (var order in orderData)
        {
            // Add to totals
            totalGtv += order.Gtv;
            totalPrice += order.Price;

            // Add to monthly data
            string monthName = order.StartDate.ToString("MMM yyyy"); // Include year for clarity
            if (monthlyData.ContainsKey(monthName))
            {
                var current = monthlyData[monthName];
                monthlyData[monthName] = (
                    income: current.income + order.Gtv,
                    revenue: current.revenue + (order.Gtv - order.Price), // Correct revenue calculation
                    payout: current.payout + order.Price
                );
            }
        }

        // Calculate total revenue (GTV - Price)
        float totalRevenue = totalGtv - totalPrice;

        // Convert monthly data to chart format, ordered chronologically
        var chartData = monthlyData
            .OrderBy(m => DateTime.ParseExact(m.Key, "MMM yyyy", null))
            .Select(m => new LineChartEntry
            {
                Name = m.Key,
                Income = m.Value.income,     // GTV for the month
                Revenue = m.Value.revenue,   // (GTV - Price) for the month
                Payout = m.Value.payout      // Price for the month
            })
            .ToList();

        var summary = new GtvDashboardSummary
        {
            TotalGtv = totalGtv,
            TotalRevenue = totalRevenue,  // Correctly calculated as GTV - Price
            TotalPayout = totalPrice,
            MonthlyData = chartData
        };

        return new ApiResponseModel<GtvDashboardSummary>
        {
            Data = summary,
            IsSuccessful = true,
            Message = "GTV dashboard data retrieved successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        return new ApiResponseModel<GtvDashboardSummary>
        {
            Data = null,
            IsSuccessful = false,
            Message = $"Error retrieving GTV dashboard data: {ex.Message}",
            StatusCode = 500
        };
    }
}
private Dictionary<string, (float income, float revenue, float payout)> InitializeMonthlyGtvData(DateTime startDate, DateTime endDate)
{
    var monthlyData = new Dictionary<string, (float income, float revenue, float payout)>();

    // Generate all months between startDate and endDate
    DateTime currentDate = new DateTime(startDate.Year, startDate.Month, 1);
    DateTime endMonth = new DateTime(endDate.Year, endDate.Month, 1);

    while (currentDate <= endMonth)
    {
        string monthName = currentDate.ToString("MMM yyyy");
        monthlyData[monthName] = (0f, 0f, 0f); // Initialize with zeros
        currentDate = currentDate.AddMonths(1);
    }

    return monthlyData;
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

    try
    {
        // Initialize monthly data structure
        var monthlyData = InitializeMonthlyData(startDate, endDate);

        // Get Nigeria orders (Order entity) in parallel
        var nigeriaStats =await GetNigeriaOrderStats(startDate, endDate, monthlyData);
      
        // Get US orders (CargoOrders entity) in parallel
        var usStats = await GetUSOrderStats(startDate, endDate, monthlyData);

        

        // Combine monthly data from both countries
        var combinedMonthlyData = CombineMonthlyStats(nigeriaStats.MonthlyData, usStats.MonthlyData);

        // Create comprehensive response
        var stats = new OrderStatsResponse
        {
            // Combined totals
            CompletedOrders = nigeriaStats.CompletedOrders + usStats.CompletedOrders,
            FlaggedOrders = nigeriaStats.FlaggedOrders + usStats.FlaggedOrders,
            InTransitOrders = nigeriaStats.InTransitOrders + usStats.InTransitOrders,
            TotalOrders = nigeriaStats.TotalOrders + usStats.TotalOrders,
            
            // Country-specific breakdown
            NigeriaStats = nigeriaStats,
            USStats = usStats,
            
            // Combined monthly data
            MonthlyData = combinedMonthlyData,
            
            // Date range info
            StartDate = startDate,
            EndDate = endDate,
            DateRange = DateTime.UtcNow
        };

        return new ApiResponseModel<OrderStatsResponse>
        {
            Data = stats,
            IsSuccessful = true,
            Message = "Order statistics retrieved successfully for both Nigeria and US operations",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        return new ApiResponseModel<OrderStatsResponse>
        {
            Data = null,
            IsSuccessful = false,
            Message = $"Error retrieving order statistics: {ex.Message}",
            StatusCode = 500
        };
    }
}
private Dictionary<string, (int completed, int flagged, int inTransit, int total, int nigeria, int us)> 
    InitializeMonthlyData(DateTime startDate, DateTime endDate)
{
    var monthlyData = new Dictionary<string, (int, int, int, int, int, int)>();
    
    DateTime currentDate = new DateTime(startDate.Year, startDate.Month, 1);
    DateTime endMonth = new DateTime(endDate.Year, endDate.Month, 1);

    while (currentDate <= endMonth)
    {
        string monthName = currentDate.ToString("MMM yyyy");
        monthlyData[monthName] = (0, 0, 0, 0, 0, 0);
        currentDate = currentDate.AddMonths(1);
    }

    return monthlyData;
}

private async Task<CountryOrderStats> GetNigeriaOrderStats(DateTime startDate, DateTime endDate, 
    Dictionary<string, (int completed, int flagged, int inTransit, int total, int nigeria, int us)> monthlyData)
{
    // Single optimized query for Nigeria orders (Order entity)
    var orders = await _context.Orders
        .Where(o => o.StartDate >= startDate && o.EndDate <= endDate)
        .Select(o => new { 
            o.StartDate, 
            o.OrderStatus 
        })
        .ToListAsync();

    var stats = new CountryOrderStats
    {
        Country = "Nigeria",
        CompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered),
        FlaggedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Flagged),
        InTransitOrders = orders.Count(o => o.OrderStatus == OrderStatus.InTransit),
        TotalOrders = orders.Count
    };

    // Update monthly data for Nigeria
    foreach (var order in orders)
    {
        string monthName = order.StartDate.ToString("MMM yyyy");
        if (monthlyData.ContainsKey(monthName))
        {
            var current = monthlyData[monthName];
            int completedCount = order.OrderStatus == OrderStatus.Delivered ? 1 : 0;
            int flaggedCount = order.OrderStatus == OrderStatus.Flagged ? 1 : 0;
            int inTransitCount = order.OrderStatus == OrderStatus.InTransit ? 1 : 0;

            monthlyData[monthName] = (
                current.completed + completedCount,
                current.flagged + flaggedCount,
                current.inTransit + inTransitCount,
                current.total + 1,
                current.nigeria + 1,
                current.us
            );
        }
    }
    
    stats.MonthlyData = monthlyData.Select(m => new MonthlyOrderStats
        {
            Month = m.Key,
            CompletedOrders = m.Value.completed,
            FlaggedOrders = m.Value.flagged,
            InTransitOrders = m.Value.inTransit,
            TotalOrders = m.Value.total,
            NigeriaOrders = m.Value.nigeria,
            USOrders = m.Value.us
        })
        .OrderBy(m => DateTime.ParseExact(m.Month, "MMM yyyy", null))
        .ToList();

    return stats;
}

private async Task<CountryOrderStats> GetUSOrderStats(DateTime startDate, DateTime endDate,
    Dictionary<string, (int completed, int flagged, int inTransit, int total, int nigeria, int us)> monthlyData)
{
    // Single optimized query for US orders (CargoOrders entity)
    var cargoOrders = await _context.CargoOrders
        .Where(co => co.CreatedAt >= startDate && co.CreatedAt <= endDate)
        .Select(co => new { 
            co.CreatedAt, 
            co.Status, 
            co.IsFlagged 
        })
        .ToListAsync();

    var stats = new CountryOrderStats
    {
        Country = "United States",
        CompletedOrders = cargoOrders.Count(co => co.Status == CargoOrderStatus.Delivered || co.Status == CargoOrderStatus.Completed),
        FlaggedOrders = cargoOrders.Count(co => co.IsFlagged),
        InTransitOrders = cargoOrders.Count(co => co.Status == CargoOrderStatus.InTransit),
        TotalOrders = cargoOrders.Count
    };

    // Update monthly data for US
    foreach (var order in cargoOrders)
    {
        string monthName = order.CreatedAt.ToString("MMM yyyy");
        if (monthlyData.ContainsKey(monthName))
        {
            var current = monthlyData[monthName];
            int completedCount = (order.Status == CargoOrderStatus.Delivered || order.Status == CargoOrderStatus.Completed) ? 1 : 0;
            int flaggedCount = order.IsFlagged ? 1 : 0;
            int inTransitCount = order.Status == CargoOrderStatus.InTransit ? 1 : 0;

            monthlyData[monthName] = (
                current.completed + completedCount,
                current.flagged + flaggedCount,
                current.inTransit + inTransitCount,
                current.total + 1,
                current.nigeria,
                current.us + 1
            );
        }
    }
    stats.MonthlyData = monthlyData.Select(m => new MonthlyOrderStats
        {
            Month = m.Key,
            CompletedOrders = m.Value.completed,
            FlaggedOrders = m.Value.flagged,
            InTransitOrders = m.Value.inTransit,
            TotalOrders = m.Value.total,
            NigeriaOrders = m.Value.nigeria,
            USOrders = m.Value.us
        })
        .OrderBy(m => DateTime.ParseExact(m.Month, "MMM yyyy", null))
        .ToList();

    return stats;
}

private List<MonthlyOrderStats> CombineMonthlyStats(List<MonthlyOrderStats> nigeriaMonthly, List<MonthlyOrderStats> usMonthly)
{
    // Avoid null references
    nigeriaMonthly ??= new List<MonthlyOrderStats>();
    usMonthly ??= new List<MonthlyOrderStats>();

    // Merge by month
    var monthlyData = nigeriaMonthly
        .Union(usMonthly)
        .GroupBy(m => m.Month)
        .ToDictionary(
            g => g.Key,
            g => new
            {
                completed = g.Sum(x => x.CompletedOrders),
                flagged = g.Sum(x => x.FlaggedOrders),
                inTransit = g.Sum(x => x.InTransitOrders),
                total = g.Sum(x => x.TotalOrders),
                nigeria = g.Where(x => nigeriaMonthly.Contains(x)).Sum(x => x.TotalOrders),
                us = g.Where(x => usMonthly.Contains(x)).Sum(x => x.TotalOrders)
            }
        );

    return monthlyData.Select(m => new MonthlyOrderStats
        {
            Month = m.Key,
            CompletedOrders = m.Value.completed,
            FlaggedOrders = m.Value.flagged,
            InTransitOrders = m.Value.inTransit,
            TotalOrders = m.Value.total,
            NigeriaOrders = m.Value.nigeria,
            USOrders = m.Value.us
        })
        .OrderBy(m => DateTime.ParseExact(m.Month, "MMM yyyy", null))
        .ToList();
}


private Dictionary<string, (int completed, int flagged, int inTransit, int total, int nigeria, int us)> monthlyData;
   
public async Task<ApiResponseModel<CargoFinancialSummaryResponse>> GetCargoFinancialSummary(DateTime startDate, DateTime endDate)
{
    if (startDate > endDate)
    {
        return new ApiResponseModel<CargoFinancialSummaryResponse>
        {
            Data = null,
            IsSuccessful = false,
            Message = "Start date must be less than or equal to end date",
            StatusCode = 400
        };
    }

    try
    {
        // Get all relevant cargo orders with related data in date range
        var ordersData = await _context.CargoOrders
            .Where(co => co.CreatedAt >= startDate && co.CreatedAt <= endDate)
            //.Where(co => co.AcceptedBidId != null) // Only orders with accepted bids
            .Select(co => new
            {
                co.Id,
                co.CreatedAt,
                co.Status,
                co.TotalAmount,
                co.SystemFee,
                co.Tax,
                co.PaymentMethod,
                co.PaymentStatus,
                co.IsPaid,
                co.PaymentDate,
                co.WalletPaymentAmount,
                co.StripePaymentAmount,
                co.DriverEarnings,
                CargoOwnerType = co.CargoOwner.OwnerType,
                CargoOwnerId = co.CargoOwnerId
            })
            .ToListAsync();

        // Get invoice data
        var invoicesData = await _context.Set<Invoice>()
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
            .Select(i => new
            {
                i.Id,
                i.CreatedAt,
                i.TotalAmount,
                i.SubTotal,
                i.SystemFee,
                i.Tax,
                i.Status,
                i.DueDate,
                i.PaymentApprovedAt,
                OrderCargoOwnerType = i.Order.CargoOwner.OwnerType
            })
            .ToListAsync();

        // Get driver wallet transactions for payout tracking
        var driverPayouts = await _context.Set<DriverWalletTransaction>()
            .Where(dwt => dwt.TransactionType == DriverTransactionType.Delivery)
            .Where(dwt => dwt.CreatedAt >= startDate && dwt.CreatedAt <= endDate)
            .Select(dwt => new
            {
                dwt.Amount,
                dwt.CreatedAt,
                dwt.IsProcessed
            })
            .ToListAsync();

        // Calculate core metrics
        var totalSystemFees = ordersData.Sum(o => o.SystemFee);
        var totalTaxCollected = ordersData.Sum(o => o.Tax);
        var totalCompanyRevenue = totalSystemFees + totalTaxCollected;

        var totalCustomerPayments = ordersData.Where(o => o.IsPaid).Sum(o => o.TotalAmount + o.SystemFee + o.Tax);
        var totalDriverPayouts = driverPayouts.Where(dp => dp.IsProcessed).Sum(dp => dp.Amount);
        var pendingDriverPayouts = driverPayouts.Where(dp => !dp.IsProcessed).Sum(dp => dp.Amount);

        // Payment method breakdown
        var paymentMethods = CalculatePaymentMethodBreakdown(ordersData);

        // Invoice summary  
        var invoicesSummary = CalculateInvoicesSummary(invoicesData);

        // Customer type analysis
        var shipperStats = CalculateCustomerTypeFinancials(ordersData, invoicesData, CargoOwnerType.Shipper);
        var brokerStats = CalculateCustomerTypeFinancials(ordersData, invoicesData, CargoOwnerType.Broker);

        // Performance metrics
        var performance = await CalculatePerformanceMetrics(ordersData, startDate, endDate);

        // Monthly breakdown
        var monthlyBreakdown = CalculateMonthlyBreakdown(ordersData, invoicesData, driverPayouts, startDate, endDate);

        var summary = new CargoFinancialSummaryResponse
        {
            TotalSystemFees = totalSystemFees,
            TotalTaxCollected = totalTaxCollected,
            TotalCompanyRevenue = totalCompanyRevenue,
            TotalCustomerPayments = totalCustomerPayments,
            TotalDriverPayouts = totalDriverPayouts,
            PendingDriverPayouts = pendingDriverPayouts,
            PaymentMethods = paymentMethods,
            InvoicesSummary = invoicesSummary,
            ShipperStats = shipperStats,
            BrokerStats = brokerStats,
            Performance = performance,
            MonthlyBreakdown = monthlyBreakdown,
            GeneratedAt = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Period = $"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}"
        };

        return new ApiResponseModel<CargoFinancialSummaryResponse>
        {
            Data = summary,
            IsSuccessful = true,
            Message = "Cargo financial summary retrieved successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        return new ApiResponseModel<CargoFinancialSummaryResponse>
        {
            Data = null,
            IsSuccessful = false,
            Message = $"Error retrieving cargo financial summary: {ex.Message}",
            StatusCode = 500
        };
    }
}
private PaymentMethodBreakdown CalculatePaymentMethodBreakdown(IEnumerable<dynamic> ordersData)
{
    var ordersList = ordersData.ToList();
    var paidOrders = ordersList.Where(o => (bool)o.IsPaid).ToList();
    var totalPayments = paidOrders.Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);
    
    var stripePayments = paidOrders
        .Where(o => (PaymentMethodType)o.PaymentMethod == PaymentMethodType.Stripe)
        .Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);
    
    var walletPayments = paidOrders
        .Where(o => (PaymentMethodType)o.PaymentMethod == PaymentMethodType.Wallet)
        .Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);
    
    var invoicePayments = paidOrders
        .Where(o => (PaymentMethodType)o.PaymentMethod == PaymentMethodType.Invoice)
        .Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);
    
    var mixedPayments = paidOrders
        .Where(o => (PaymentMethodType)o.PaymentMethod == PaymentMethodType.Mixed)
        .Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);

    return new PaymentMethodBreakdown
    {
        StripePayments = stripePayments,
        WalletPayments = walletPayments,
        InvoicePayments = invoicePayments,
        MixedPayments = mixedPayments,
        StripePercentage = totalPayments > 0 ? Math.Round((stripePayments / totalPayments) * 100, 2) : 0,
        WalletPercentage = totalPayments > 0 ? Math.Round((walletPayments / totalPayments) * 100, 2) : 0,
        InvoicePercentage = totalPayments > 0 ? Math.Round((invoicePayments / totalPayments) * 100, 2) : 0,
        MixedPercentage = totalPayments > 0 ? Math.Round((mixedPayments / totalPayments) * 100, 2) : 0
    };
}
private InvoiceStatusSummary CalculateInvoicesSummary(IEnumerable<dynamic> invoicesData)
{
    var invoicesList = invoicesData.ToList();
    var pendingInvoices = invoicesList.Where(i => (InvoiceStatus)i.Status == InvoiceStatus.Pending).ToList();
    var overdueInvoices = invoicesList.Where(i => (InvoiceStatus)i.Status == InvoiceStatus.Overdue || 
        ((InvoiceStatus)i.Status == InvoiceStatus.Pending && (DateTime)i.DueDate < DateTime.UtcNow)).ToList();
    var paidInvoices = invoicesList.Where(i => (InvoiceStatus)i.Status == InvoiceStatus.Paid).ToList();
    var cancelledInvoices = invoicesList.Where(i => (InvoiceStatus)i.Status == InvoiceStatus.Cancelled).ToList();

    var totalInvoiceAmount = invoicesList.Sum(i => (decimal)i.TotalAmount);
    var paidAmount = paidInvoices.Sum(i => (decimal)i.TotalAmount);

    return new InvoiceStatusSummary
    {
        PendingAmount = pendingInvoices.Sum(i => (decimal)i.TotalAmount),
        OverdueAmount = overdueInvoices.Sum(i => (decimal)i.TotalAmount),
        PaidAmount = paidAmount,
        CancelledAmount = cancelledInvoices.Sum(i => (decimal)i.TotalAmount),
        PendingCount = pendingInvoices.Count,
        OverdueCount = overdueInvoices.Count,
        PaidCount = paidInvoices.Count,
        CancelledCount = cancelledInvoices.Count,
        TotalInvoices = invoicesList.Count,
        CollectionRate = totalInvoiceAmount > 0 ? Math.Round((paidAmount / totalInvoiceAmount) * 100, 2) : 0,
        OverdueRate = totalInvoiceAmount > 0 ? Math.Round((overdueInvoices.Sum(i => (decimal)i.TotalAmount) / totalInvoiceAmount) * 100, 2) : 0
    };
}

private CustomerTypeFinancials CalculateCustomerTypeFinancials(IEnumerable<dynamic> ordersData, IEnumerable<dynamic> invoicesData, CargoOwnerType customerType)
{
    var ordersList = ordersData.ToList();
    var invoicesList = invoicesData.ToList();
    
    var customerOrders = ordersList.Where(o => (CargoOwnerType)o.CargoOwnerType == customerType).ToList();
    var customerInvoices = invoicesList.Where(i => (CargoOwnerType)i.OrderCargoOwnerType == customerType).ToList();
    
    var completedOrders = customerOrders.Where(o => (CargoOrderStatus)o.Status == CargoOrderStatus.Completed || 
                                                   (CargoOrderStatus)o.Status == CargoOrderStatus.Delivered).ToList();
    var paidOrders = customerOrders.Where(o => (bool)o.IsPaid).ToList();
    
    var totalRevenue = customerOrders.Sum(o => (decimal)o.SystemFee);
    var totalPayments = paidOrders.Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);
    var pendingPayments = customerOrders.Where(o => !(bool)o.IsPaid && (PaymentStatus)o.PaymentStatus == PaymentStatus.Pending)
                                       .Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);
    var overduePayments = customerOrders.Where(o => !(bool)o.IsPaid && (PaymentStatus)o.PaymentStatus == PaymentStatus.Overdue)
                                       .Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax);

    // Calculate average payment time
    var paidOrdersWithDates = paidOrders.Where(o => o.PaymentDate != null).ToList();
    var averagePaymentDays = paidOrdersWithDates.Any() 
        ? paidOrdersWithDates.Average(o => ((DateTime?)o.PaymentDate).Value.Subtract((DateTime)o.CreatedAt).TotalDays)
        : 0;

    return new CustomerTypeFinancials
    {
        CustomerType = customerType.ToString(),
        TotalOrders = customerOrders.Count,
        CompletedOrders = completedOrders.Count,
        TotalRevenue = totalRevenue,
        TotalPayments = totalPayments,
        PendingPayments = pendingPayments,
        OverduePayments = overduePayments,
        AverageOrderValue = customerOrders.Any() ? customerOrders.Average(o => (decimal)o.TotalAmount) : 0,
        AverageSystemFee = customerOrders.Any() ? customerOrders.Average(o => (decimal)o.SystemFee) : 0,
        PaymentSuccessRate = customerOrders.Any() ? Math.Round((decimal)paidOrders.Count / customerOrders.Count * 100, 2) : 0,
        AveragePaymentDays = averagePaymentDays
    };
}

private async Task<PerformanceMetrics> CalculatePerformanceMetrics(IEnumerable<dynamic> ordersData, DateTime startDate, DateTime endDate)
{
    var ordersList = ordersData.ToList();
    
    // Get previous period for growth calculation
    var periodDays = (endDate - startDate).Days;
    var previousStart = startDate.AddDays(-periodDays);
    var previousEnd = startDate;

    var previousPeriodRevenue = await _context.CargoOrders
        .Where(co => co.CreatedAt >= previousStart && co.CreatedAt < previousEnd)
        .Where(co => co.AcceptedBidId != null)
        .SumAsync(co => co.SystemFee);

    var currentRevenue = ordersList.Sum(o => (decimal)o.SystemFee);
    var revenueGrowthRate = previousPeriodRevenue > 0 
        ? Math.Round(((currentRevenue - previousPeriodRevenue) / previousPeriodRevenue) * 100, 2)
        : currentRevenue > 0 ? 100 : 0;

    var paidOrders = ordersList.Where(o => (bool)o.IsPaid).ToList();
    var paymentSuccessRate = ordersList.Any() ? Math.Round((decimal)paidOrders.Count / ordersList.Count * 100, 2) : 0;

    var ordersWithPaymentDates = paidOrders.Where(o => o.PaymentDate != null).ToList();
    var averageCollectionTime = ordersWithPaymentDates.Any()
        ? ordersWithPaymentDates.Average(o => ((DateTime?)o.PaymentDate).Value.Subtract((DateTime)o.CreatedAt).TotalDays)
        : 0;

    // Get unique customers
    var activeCustomers = ordersList.Select(o => (string)o.CargoOwnerId).Distinct().Count();

    // Get new customers (first order in this period)
    var allCustomerIds = ordersList.Select(o => (string)o.CargoOwnerId).Distinct().ToList();
    var newCustomersCount = 0;
    foreach (var customerId in allCustomerIds)
    {
        var firstOrder = await _context.CargoOrders
            .Where(co => co.CargoOwnerId == customerId)
            .OrderBy(co => co.CreatedAt)
            .FirstOrDefaultAsync();
        
        if (firstOrder?.CreatedAt >= startDate && firstOrder?.CreatedAt <= endDate)
        {
            newCustomersCount++;
        }
    }

    return new PerformanceMetrics
    {
        RevenueGrowthRate = revenueGrowthRate,
        AverageRevenuePerOrder = ordersList.Any() ? ordersList.Average(o => (decimal)o.SystemFee) : 0,
        PaymentSuccessRate = paymentSuccessRate,
        AverageCollectionTime = averageCollectionTime,
        CustomerRetentionRate = activeCustomers > 0 ? Math.Round((decimal)(activeCustomers - newCustomersCount) / activeCustomers * 100, 2) : 0,
        ActiveCustomers = activeCustomers,
        NewCustomers = newCustomersCount
    };
}
private List<MonthlyFinancialData> CalculateMonthlyBreakdown(IEnumerable<dynamic> ordersData, IEnumerable<dynamic> invoicesData, IEnumerable<dynamic> driverPayouts, DateTime startDate, DateTime endDate)
{
    var monthlyData = new List<MonthlyFinancialData>();
    var ordersList = ordersData.ToList();
    var invoicesList = invoicesData.ToList();
    var payoutsList = driverPayouts.ToList();

    // Initialize all months in the range
    var current = new DateTime(startDate.Year, startDate.Month, 1);
    var end = new DateTime(endDate.Year, endDate.Month, 1);

    while (current <= end)
    {
        var monthStart = current;
        var monthEnd = current.AddMonths(1).AddDays(-1);

        var monthOrders = ordersList.Where(o => (DateTime)o.CreatedAt >= monthStart && (DateTime)o.CreatedAt <= monthEnd).ToList();
        var monthInvoices = invoicesList.Where(i => (DateTime)i.CreatedAt >= monthStart && (DateTime)i.CreatedAt <= monthEnd).ToList();
        var monthDriverPayouts = payoutsList.Where(dp => (DateTime)dp.CreatedAt >= monthStart && (DateTime)dp.CreatedAt <= monthEnd).ToList();

        var monthlyFinancials = new MonthlyFinancialData
        {
            Month = current.ToString("MMM yyyy"),
            Year = current.Year,
            MonthNumber = current.Month,
            Revenue = monthOrders.Sum(o => (decimal)o.SystemFee),
            CustomerPayments = monthOrders.Where(o => (bool)o.IsPaid).Sum(o => (decimal)o.TotalAmount + (decimal)o.SystemFee + (decimal)o.Tax),
            DriverPayouts = monthDriverPayouts.Where(dp => (bool)dp.IsProcessed).Sum(dp => (decimal)dp.Amount),
            NewInvoices = monthInvoices.Sum(i => (decimal)i.TotalAmount),
            InvoicesPaid = monthInvoices.Where(i => (InvoiceStatus)i.Status == InvoiceStatus.Paid).Sum(i => (decimal)i.TotalAmount),
            CompletedOrders = monthOrders.Count(o => (CargoOrderStatus)o.Status == CargoOrderStatus.Completed || (CargoOrderStatus)o.Status == CargoOrderStatus.Delivered),
            NewCustomers = monthOrders.Select(o => (string)o.CargoOwnerId).Distinct().Count(),
            ShipperRevenue = monthOrders.Where(o => (CargoOwnerType)o.CargoOwnerType == CargoOwnerType.Shipper).Sum(o => (decimal)o.SystemFee),
            BrokerRevenue = monthOrders.Where(o => (CargoOwnerType)o.CargoOwnerType == CargoOwnerType.Broker).Sum(o => (decimal)o.SystemFee)
        };

        monthlyData.Add(monthlyFinancials);
        current = current.AddMonths(1);
    }

    return monthlyData.OrderBy(m => m.Year).ThenBy(m => m.MonthNumber).ToList();
}

}