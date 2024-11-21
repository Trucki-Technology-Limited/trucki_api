using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using trucki.CustomExtension;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class ManagerRepository : IManagerRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public ManagerRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
    }
    public async Task<ApiResponseModel<string>> AddManager(AddManagerRequestModel model)
    {
        var existingManager = await _context.Managers
            .FirstOrDefaultAsync(m => m.EmailAddress == model.EmailAddress || m.Phone == model.Phone);

        if (existingManager != null)
        {
            if (existingManager.EmailAddress == model.EmailAddress)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Email address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
            else
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Phone number already exists",
                    StatusCode = 400 // Bad Request
                };
            }
        }

        var newManager = new Manager
        {
            Name = model.Name,
            Phone = model.Phone,
            EmailAddress = model.EmailAddress,
            Company = new List<Business>(), // Initialize empty company list
            ManagerType = model.ManagerType
        };

        // **Add companies to manager**
        foreach (var companyId in model.CompanyId)
        {
            var business = await _context.Businesses.FindAsync(companyId);
            if (business != null)
            {
                newManager.Company.Add(business);
            }
        }

        _context.Managers.Add(newManager);
        var password = HelperClass.GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newManager.Name, newManager.EmailAddress, newManager.Phone,
            newManager.ManagerType == 0 ? "manager" : "finance", password, false);
        //TODO:: Email password to user
        if (res.StatusCode == 201)
        {
            var user = await _userManager.FindByEmailAsync(newManager.EmailAddress);
            newManager.UserId = user.Id;
            var emailSubject = "Account Created";
            await _emailSender.SendEmailAsync(newManager.EmailAddress, emailSubject, password);
            // **Save changes to database**
            await _context.SaveChangesAsync();
            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Manager created successfully",
                StatusCode = 201,
                Data = password
            };
        }

        return new ApiResponseModel<string>
        {
            IsSuccessful = false,
            Message = "An error occurred while creating the manager",
            StatusCode = 400, // Bad Request
        };
    }

    public async Task<ApiResponseModel<List<AllManagerResponseModel>>> GetAllManager()
    {
        var managers = await _context.Managers.ToListAsync();

        var managersResponseModels = _mapper.Map<List<AllManagerResponseModel>>(managers);

        return new ApiResponseModel<List<AllManagerResponseModel>>
        {
            IsSuccessful = true,
            Message = "Businesses retrieved successfully",
            StatusCode = 200,
            Data = managersResponseModels
        };
    }

    public async Task<ApiResponseModel<AllManagerResponseModel>> GetManagerById(string id)
    {
        var manager = await _context.Managers.Where(x => x.Id == id).Include(b => b.Company).FirstOrDefaultAsync();

        if (manager == null)
            return new ApiResponseModel<AllManagerResponseModel>
            {
                Data = new AllManagerResponseModel { },
                IsSuccessful = false,
                Message = "No manager found",
                StatusCode = 404
            };

        var managerToReturn = _mapper.Map<AllManagerResponseModel>(manager);


        return new ApiResponseModel<AllManagerResponseModel>
        {
            IsSuccessful = true,
            Message = "Manager retrieved successfully",
            StatusCode = 200,
            Data = managerToReturn
        };
    }

    public async Task<ApiResponseModel<bool>> EditManager(EditManagerRequestModel model)
    {
        var manager = await _context.Managers.FindAsync(model.ManagerId);
        if (manager == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }

        manager.Name = model.Name;
        manager.Phone = model.Phone;
        manager.EmailAddress = model.EmailAddress;
        // Update company list logic can be added here similar to AddManager

        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Manager details updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> DeactivateManager(string managerId)
    {
        var manager = await _context.Managers
            .Include(m => m.Company) // Eagerly load the Company collection
            .Include(e => e.User) // Eagerly load the Company collection
            .FirstOrDefaultAsync(m => m.Id == managerId);
        if (manager == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }
        // Fetch orders related to the selected manager
        var orders = await _context.Orders
            .Where(o => o.ManagerId == managerId)
            .ToListAsync();

        // Set ManagerId to null in all orders
        foreach (var order in orders)
        {
            order.ManagerId = null;
            order.Manager = null; // Optional, just to clear navigation property
        }
        // Get attached businesses
        var businesses = manager.Company.ToList();

        // Remove manager from businesses
        foreach (var business in businesses)
        {
            business.Manager = null;
            business.managerId = null;
        }

        if (manager.UserId != null)
        {
            var user = await _userManager.FindByIdAsync(manager.UserId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }
        // Remove manager
        _context.Managers.Remove(manager);

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Manager deactivated successfully",
            StatusCode = 200,
            Data = true
        };
    }
    public async Task<ApiResponseModel<IEnumerable<AllManagerResponseModel>>> SearchManagers(string searchWords)
    {
        IQueryable<Manager> query = _context.Managers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " &&
            searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.Name.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var drivers = await query.ToListAsync();

        if (!drivers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllManagerResponseModel>>
            {
                Data = new List<AllManagerResponseModel> { },
                IsSuccessful = false,
                Message = "No manager found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllManagerResponseModel>>(drivers);

        return new ApiResponseModel<IEnumerable<AllManagerResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Managers successfully retrieved",
            StatusCode = 200,
        };
    }



    public async Task<string> GetManagerIdAsync(string? userId)
    {
        var ManagerId = "";
        if (string.IsNullOrEmpty(userId))
        {
            return userId;
        }
        else
        {
            var manager = await _context.Managers.FindAsync(userId);
            if (manager != null)
            {
                ManagerId = manager.Id;
            }

            else if (manager == null)
            {
                ManagerId = "";
            }
        }

        return ManagerId;
    }
    public async Task<ApiResponseModel<ManagerDashboardData>> GetManagerDashboardData(List<string> userRoles,
string userId)
    {

        // Determine if the user has high-level roles
        bool isManager = userRoles.Any(role => role.Equals("manager", StringComparison.OrdinalIgnoreCase));
        bool isFieldOfficer = userRoles.Any(role => role.Equals("field officer", StringComparison.OrdinalIgnoreCase));


        // Initialize the query for orders
        IQueryable<Order> ordersQuery = _context.Orders
            .Include(o => o.Routes);

        if (isManager || isFieldOfficer)
        {
            // Map userId to ManagerId
            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive);

            if (manager == null)
            {
                return new ApiResponseModel<ManagerDashboardData>
                {
                    Data = null,
                    IsSuccessful = false,
                    Message = "Manager not found for the given user.",
                    StatusCode = 404
                };
            }

            // Retrieve Business IDs managed by this manager
            var managedBusinessIds = await _context.Businesses
                .Where(b => b.managerId == manager.Id && b.isActive)
                .Select(b => b.Id)
                .ToListAsync();

            if (managedBusinessIds == null || !managedBusinessIds.Any())
            {
                // No businesses managed by this manager
                return new ApiResponseModel<ManagerDashboardData>
                {
                    Data = new ManagerDashboardData
                    {
                        CompletedOrders = 0,
                        FlaggedOrders = 0,
                        InTransitOrders = 0,
                        TotalOrder = 0
                    },
                    IsSuccessful = true,
                    Message = "No businesses managed by this manager.",
                    StatusCode = 200
                };
            }

            // Filter orders based on managed businesses
            ordersQuery = ordersQuery.Where(o => managedBusinessIds.Contains(o.BusinessId));
        }
        // Else, if user is Chief Manager or Finance, retrieve all orders within date range (no additional filter)

        // Execute the query
        List<Order> orders = await ordersQuery.ToListAsync();

        int completedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
        int flaggedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Flagged);
        int inTransitCount = orders.Count(o => o.OrderStatus == OrderStatus.InTransit);
        int totalOrder = orders.Count;

        var stats = new ManagerDashboardData
        {
            CompletedOrders = completedOrders,
            FlaggedOrders = flaggedOrders,
            InTransitOrders = inTransitCount,
            TotalOrder = totalOrder
        };

        return new ApiResponseModel<ManagerDashboardData>
        {
            Data = stats,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<List<TransactionResponseModel>>> GetTransactionsByManager(List<string> userRoles, string userId)
    {
        bool isManager = userRoles.Any(role => role.Equals("manager", StringComparison.OrdinalIgnoreCase));
        bool isFieldOfficer = userRoles.Any(role => role.Equals("field officer", StringComparison.OrdinalIgnoreCase));

        List<string> businessIds = new List<string>();

        if (isManager)
        {
            // Get the manager
            var manager = await _context.Managers.Include(e => e.Company).FirstOrDefaultAsync(e => e.UserId == userId);
            if (manager == null)
            {
                return new ApiResponseModel<List<TransactionResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "Manager not found",
                    StatusCode = 404 // Not Found
                };
            }

            // Get the businesses managed by this manager
            businessIds = await _context.Businesses
                .Where(b => b.managerId == manager.Id)
                .Select(b => b.Id)
                .ToListAsync();
        }
        else if (isFieldOfficer)
        {
            // Get the officer
            var officer = await _context.Officers.FirstOrDefaultAsync(e => e.UserId == userId);
            if (officer == null)
            {
                return new ApiResponseModel<List<TransactionResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "Officer not found",
                    StatusCode = 404 // Not Found
                };
            }

            // Get the businesses under the officer's company
            businessIds = await _context.Businesses
                .Where(b => b.Id == officer.CompanyId)
                .Select(b => b.Id)
                .ToListAsync();
        }
        else
        {
            // If the user has no specific roles, fetch all businesses
            businessIds = await _context.Businesses
                .Select(b => b.Id)
                .ToListAsync();
        }

        // Retrieve transactions that belong to those businesses
        var transactions = _context.Transactions
            .Where(t => businessIds.Contains(t.BusinessId))
            .Include(t => t.Order)
            .Include(t => t.Truck).ThenInclude(e => e.TruckOwner)
            .Include(t => t.Business) // If you want to include the Business details
            .ToList();
        var responseModels = transactions.Select(t => new TransactionResponseModel
        {
            TransactionId = t.Id,
            TransactionDate = t.TransactionDate,
            TransactionType = t.Type,
            Amount = t.Amount,
            OrderId = t.Order.Id,
            CargoType = t.Order.CargoType,
            OrderStatus = t.Order.OrderStatus,
            BusinessId = t.Business.Id,
            BusinessName = t.Business.Name,
            truckOwner = t.Truck?.TruckOwner?.Name,
            TruckId = t.Truck?.Id, // Use null-conditional operator to handle potential null
            TruckNo = t.Truck?.PlateNumber
        }).ToList();
        return new ApiResponseModel<List<TransactionResponseModel>>
        {
            Data = responseModels,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200
        };
    }
    public async Task<ApiResponseModel<TransactionSummaryResponseModel>> GetTransactionSummaryResponseModel(string userId)
    {
        var manager = await _context.Managers.Include(e => e.Company).Where(e => e.UserId == userId).FirstOrDefaultAsync();
        if (manager == null)
        {
            return new ApiResponseModel<TransactionSummaryResponseModel>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }
        // Get the businesses managed by this manager
        var managedBusinesses = await _context.Businesses
            .Where(b => b.managerId == manager.Id)
            .Select(b => b.Id) // Get only the business IDs
            .ToListAsync();

        // Retrieve transactions that belong to those businesses
        var transactions = await _context.Transactions
            .Where(t => managedBusinesses.Contains(t.BusinessId))
            .Include(t => t.Order)
            .ThenInclude(o => o.Routes) // This will now include the Routes for each Order
            .ToListAsync();

        // Calculate total order count
        int totalOrderCount = transactions.Count;

        // Calculate total payout (60% and 40% combined)
        decimal totalPayout = transactions.Sum(t => t.Amount);

        // Calculate total GTV by summing GTV from associated orders
        decimal totalGtv = transactions.Sum(t => (decimal)t.Order.Routes.Gtv); // Assuming your Order entity has a Routes navigation property

        var response = new TransactionSummaryResponseModel
        {
            TotalOrderCount = totalOrderCount,
            TotalPayout = totalPayout,
            TotalGtv = totalGtv
        };

        return new ApiResponseModel<TransactionSummaryResponseModel>
        {
            Data = response,
            IsSuccessful = true,
            Message = "Manager dashboard data retrieved successfully",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<List<TransactionResponseModel>>> GetTransactionsByFinancialManager(string userId)
    {
        var manager = await _context.Managers.Include(e => e.Company).Where(e => e.UserId == userId).FirstOrDefaultAsync();
        if (manager == null)
        {
            return new ApiResponseModel<List<TransactionResponseModel>>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }
        // Get the businesses managed by this manager
        var managedBusinesses = await _context.Businesses
            .Where(b => b.managerId == manager.Id)
            .Select(b => b.Id) // Get only the business IDs
            .ToListAsync();

        // Retrieve transactions that belong to those businesses
        var transactions = _context.Transactions
            .Include(t => t.Order)
            .Include(t => t.Truck).ThenInclude(e => e.TruckOwner)
            .Include(t => t.Business) // If you want to include the Business details
            .ToList();
        var responseModels = transactions.Select(t => new TransactionResponseModel
        {
            TransactionId = t.Id,
            TransactionDate = t.TransactionDate,
            TransactionType = t.Type,
            Amount = t.Amount,
            OrderId = t.Order.Id,
            CargoType = t.Order.CargoType,
            OrderStatus = t.Order.OrderStatus,
            BusinessId = t.Business.Id,
            BusinessName = t.Business.Name,
            truckOwner = t.Truck.TruckOwner.Name,
            TruckId = t.Truck?.Id, // Use null-conditional operator to handle potential null
            TruckNo = t.Truck?.PlateNumber
        }).ToList();
        return new ApiResponseModel<List<TransactionResponseModel>>
        {
            Data = responseModels,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200
        };
    }
    public async Task<ApiResponseModel<TransactionSummaryResponseModel>> GetFinancialTransactionSummaryResponseModel(string userId)
    {
        var manager = await _context.Managers.Include(e => e.Company).Where(e => e.UserId == userId).FirstOrDefaultAsync();
        if (manager == null)
        {
            return new ApiResponseModel<TransactionSummaryResponseModel>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }
        // Get the businesses managed by this manager
        var managedBusinesses = await _context.Businesses
            .Where(b => b.managerId == manager.Id)
            .Select(b => b.Id) // Get only the business IDs
            .ToListAsync();

        // Retrieve transactions that belong to those businesses
        var transactions = await _context.Transactions
            .Include(t => t.Order)
            .ThenInclude(o => o.Routes) // This will now include the Routes for each Order
            .ToListAsync();

        // Calculate total order count
        int totalOrderCount = transactions.Count;

        // Calculate total payout (60% and 40% combined)
        decimal totalPayout = transactions.Sum(t => t.Amount);

        // Calculate total GTV by summing GTV from associated orders
        decimal totalGtv = transactions.Sum(t => (decimal)t.Order.Routes.Gtv); // Assuming your Order entity has a Routes navigation property

        var response = new TransactionSummaryResponseModel
        {
            TotalOrderCount = totalOrderCount,
            TotalPayout = totalPayout,
            TotalGtv = totalGtv
        };

        return new ApiResponseModel<TransactionSummaryResponseModel>
        {
            Data = response,
            IsSuccessful = true,
            Message = "Manager dashboard data retrieved successfully",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<GtvDashboardSummary>> GetManagerGtvDashBoardSummary(DateTime startDate, DateTime endDate, List<string> userRoles,
string userId)
    {
        // validate startDate and endDate
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

        // Determine if the user has high-level roles
        bool isManager = userRoles.Any(role => role.Equals("manager", StringComparison.OrdinalIgnoreCase));
        bool isFieldOfficer = userRoles.Any(role => role.Equals("field officer", StringComparison.OrdinalIgnoreCase));


        // Initialize the query for orders
        IQueryable<Order> ordersQuery = _context.Orders
            .Include(o => o.Routes)
            .Where(o => o.OrderStatus == OrderStatus.Delivered)
            .Where(o => o.StartDate >= startDate && o.EndDate <= endDate);

        if (isManager || isFieldOfficer)
        {
            // Map userId to ManagerId
            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive);

            if (manager == null)
            {
                return new ApiResponseModel<GtvDashboardSummary>
                {
                    Data = null,
                    IsSuccessful = false,
                    Message = "Manager not found for the given user.",
                    StatusCode = 404
                };
            }

            // Retrieve Business IDs managed by this manager
            var managedBusinessIds = await _context.Businesses
                .Where(b => b.managerId == manager.Id && b.isActive)
                .Select(b => b.Id)
                .ToListAsync();

            if (managedBusinessIds == null || !managedBusinessIds.Any())
            {
                // No businesses managed by this manager
                return new ApiResponseModel<GtvDashboardSummary>
                {
                    Data = new GtvDashboardSummary
                    {
                        TotalGtv = 0,
                        TotalRevenue = 0,
                        TotalPayout = 0,
                        MonthlyData = new List<LineChartEntry>()
                    },
                    IsSuccessful = true,
                    Message = "No businesses managed by this manager.",
                    StatusCode = 200
                };
            }

            // Filter orders based on managed businesses
            ordersQuery = ordersQuery.Where(o => managedBusinessIds.Contains(o.BusinessId));
        }
        // Else, if user is Chief Manager or Finance, retrieve all orders within date range (no additional filter)

        // Execute the query
        List<Order> orders = await ordersQuery.ToListAsync();

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
    public async Task<ApiResponseModel<string>> EditAssignedBusinesses(EditAssignedBusinessesRequestModel model)
    {
        var manager = await _context.Managers
            .Include(m => m.Company)
            .FirstOrDefaultAsync(m => m.Id == model.ManagerId);

        if (manager == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }

        // Step 1: Clear existing businesses and update manager reference in Business entities
        foreach (var business in manager.Company)
        {
            business.managerId = null; // Remove manager reference
            business.Manager = null;
        }
        manager.Company.Clear();

        // Step 2: Assign new businesses to the manager and update the Business entity
        foreach (var companyId in model.CompanyIds)
        {
            var business = await _context.Businesses.FindAsync(companyId);
            if (business != null)
            {
                business.managerId = model.ManagerId.ToString(); // Update manager reference
                business.Manager = manager;
                manager.Company.Add(business);
            }
        }

        // Step 3: Save changes
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Businesses updated successfully",
            StatusCode = 200 // OK
        };
    }

}