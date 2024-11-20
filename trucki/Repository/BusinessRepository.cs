using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class BusinessRepository : IBusinessRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public BusinessRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
    }
    public async Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel model)
    {
        var newBusiness = new Business
        {
            Name = model.Name,
            Location = model.Location,
            Address = model.Address,
            isActive = true
        };
        _context.Businesses.Add(newBusiness);
        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Business created successfully",
            StatusCode = 201,
            Data = true
        };
    }

    public async Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness()
    {
        var businesses = await _context.Businesses.ToListAsync();

        var businessResponseModels = _mapper.Map<List<AllBusinessResponseModel>>(businesses);

        return new ApiResponseModel<List<AllBusinessResponseModel>>
        {
            IsSuccessful = true,
            Message = "Businesses retrieved successfully",
            StatusCode = 200,
            Data = businessResponseModels
        };
    }

    public async Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model)
    {
        // Check if the business with the provided BusinessId exists
        var business = await _context.Businesses
            .Include(b => b.Routes)
            .FirstOrDefaultAsync(b => b.Id == model.BusinessId);

        if (business == null)
        {
            // If business does not exist, return appropriate response
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Business does not exist",
                StatusCode = 404,
                Data = false
            };
        }

        // Business exists, proceed to add the route
        foreach (var i in model.Routes)
        {
            var newRoute = new Routes
            {
                Name = i.Name,
                FromRoute = i.FromRoute,
                ToRoute = i.ToRoute,
                Ntons = i.Ntons,
                Price = i.Price,
                IsActive = i.IsActive,
                Gtv = i.Gtv,
                FromRouteLat = i.FromRouteLat,
                FromRouteLng = i.FromRouteLng,
                ToRouteLat = i.ToRouteLat,
                ToRouteLng = i.ToRouteLng
            };

            // Add the route to the business
            business.Routes ??= new List<Routes>();
            business.Routes.Add(newRoute);
        }

        // Save changes to the database
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Route added to business successfully",
            StatusCode = 201,
            Data = true
        };
    }

    public async Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id)
    {
        // Retrieve the business by ID including its routes
        var business = await _context.Businesses
            .Include(b => b.Routes)
            .FirstOrDefaultAsync(b => b.Id == id);

        // Check if business with the provided ID exists
        if (business == null)
        {
            return new ApiResponseModel<BusinessResponseModel>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = null
            };
        }

        // Map the business entity to a response model
        var businessResponseModel = _mapper.Map<BusinessResponseModel>(business);

        return new ApiResponseModel<BusinessResponseModel>
        {
            IsSuccessful = true,
            Message = "Business retrieved successfully",
            StatusCode = 200,
            Data = businessResponseModel
        };
    }

    public async Task<ApiResponseModel<bool>> EditBusiness(EditBusinessRequestModel model)
    {
        // Retrieve the existing business
        var business = await _context.Businesses.FindAsync(model.Id);

        if (business == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = false
            };
        }

        // Update business properties
        business.Name = model.Name;
        business.Location = model.Location;
        business.Address = model.Address;
        business.isActive = model.IsActive;

        // Save changes to the database
        try
        {
            await _context.SaveChangesAsync();
            return new ApiResponseModel<bool>
            {
                IsSuccessful = true,
                Message = "Business updated successfully",
                StatusCode = 200,
                Data = true
            };
        }
        catch (Exception ex)
        {
            // Handle potential errors during database update
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "An error occurred while updating the business",
                StatusCode = 500,
                Data = false
            };
        }
    }

    public async Task<ApiResponseModel<bool>> DeleteBusiness(string id)
    {
        // Retrieve the business to be deleted
        var business = await _context.Businesses
            .Include(b => b.Routes) // Eagerly load associated routes
            .FirstOrDefaultAsync(b => b.Id == id);

        if (business == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = false
            };
        }

        // If the business has routes, remove them before deleting the business
        if (business.Routes?.Any() == true)
        {
            _context.RoutesEnumerable.RemoveRange(business.Routes);
        }

        // Remove the business from the database
        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Business deleted successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> DisableBusiness(string id)
    {
        // Retrieve the business
        var business = await _context.Businesses.FindAsync(id);

        if (business == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = false
            };
        }

        // Disable the business by setting isActive to false
        business.isActive = false;
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Business disabled successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> EnableBusiness(string id)
    {
        // Retrieve the business
        var business = await _context.Businesses.FindAsync(id);

        if (business == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = false
            };
        }

        // Disable the business by setting isActive to false
        business.isActive = true;
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Business disabled successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> EditRoute(EditRouteRequestModel model)
    {
        // Retrieve the route to be edited
        var route = await _context.RoutesEnumerable
            .FirstOrDefaultAsync(r => r.Id == model.Id);

        if (route == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Route not found",
                StatusCode = 404,
                Data = false
            };
        }

        // Update route properties
        route.Name = model.Name;
        route.FromRoute = model.FromRoute;
        route.FromRouteLat = model.FromRouteLat;
        route.FromRouteLng = model.FromRouteLng;
        route.ToRoute = model.ToRoute;
        route.ToRouteLat = model.ToRouteLat;
        route.ToRouteLng = model.ToRouteLng;
        route.Ntons = model.Ntons;
        route.Price = model.Price;
        route.IsActive = model.IsActive;
        route.Gtv = model.Gtv;

        // Save changes to the database
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Route updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> DeleteRoute(string id)
    {
        // Retrieve the route to be deleted
        var route = await _context.RoutesEnumerable.FindAsync(id);

        if (route == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Route not found",
                StatusCode = 404,
                Data = false
            };
        }

        // Remove the route from the database
        _context.RoutesEnumerable.Remove(route);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Route deleted successfully",
            StatusCode = 200,
            Data = true
        };
    }
    public async Task<ApiResponseModel<IEnumerable<AllBusinessResponseModel>>> SearchBusinesses(string searchWords)
    {
        IQueryable<Business> query = _context.Businesses;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " &&
            searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.Name.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var drivers = await query.ToListAsync();

        if (!drivers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllBusinessResponseModel>>
            {
                Data = new List<AllBusinessResponseModel> { },
                IsSuccessful = false,
                Message = "No manager found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllBusinessResponseModel>>(drivers);

        return new ApiResponseModel<IEnumerable<AllBusinessResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Managers successfully retrieved",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<List<RouteResponseModel>>> GetRoutesByBusinessId(string businessId)
    {
        var business = await _context.Businesses
            .Include(b => b.Routes)
            .FirstOrDefaultAsync(b => b.Id == businessId);

        if (business == null)
        {
            return new ApiResponseModel<List<RouteResponseModel>>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = null
            };
        }

        var routeResponseModels = _mapper.Map<List<RouteResponseModel>>(business.Routes);

        return new ApiResponseModel<List<RouteResponseModel>>
        {
            IsSuccessful = true,
            Message = "Routes retrieved successfully",
            StatusCode = 200,
            Data = routeResponseModels
        };
    }
    public async Task<ApiResponseModel<BusinessGtvDashboardSummary>> GetBusinessGtvDashboardSummary(
    DateTime startDate, DateTime endDate, string businessId)
    {
        // Validate startDate and endDate
        if (startDate > endDate)
        {
            return new ApiResponseModel<BusinessGtvDashboardSummary>
            {
                Data = null,
                IsSuccessful = false,
                Message = "Start date must be less than or equal to end date",
                StatusCode = 400
            };
        }

        // Get orders for the specific business within the date range
        var orders = await _context.Orders
            .Include(e => e.Routes)
            .Where(o => o.BusinessId == businessId)
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

        // Convert dictionary to list for chart data
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

        // Additional metrics
        int totalOrders = orders.Count;
        int completedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
        int pendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending);
        int totalCustomers = await _context.Customers
            .Where(c => c.BusinessId == businessId)
            .CountAsync();

        // Build summary object
        var summary = new BusinessGtvDashboardSummary
        {
            TotalGtv = totalGtv,
            TotalRevenue = totalIncome,
            TotalPayout = totalPrice,
            MonthlyData = chartData,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            TotalCustomers = totalCustomers
        };

        return new ApiResponseModel<BusinessGtvDashboardSummary>
        {
            Data = summary,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200
        };
    }


}