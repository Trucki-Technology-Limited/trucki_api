using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using trucki.CustomExtension;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class DriverRepository : IDriverRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;
    private readonly ILogger<DriverRepository> _logger;

    public DriverRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender,ILogger<DriverRepository>  logger)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
        _logger = logger;
    }
    public async Task<ApiResponseModel<string>> AddDriver(AddDriverRequestModel model)
    {
        var existingDriver = await _context.Drivers
            .FirstOrDefaultAsync(m => m.EmailAddress == model.Email || m.Phone == model.Number);

        if (existingDriver != null)
        {
            if (existingDriver.EmailAddress == model.Email)
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

        var newDriver = new Driver
        {
            Name = model.Name,
            Phone = model.Number,
            EmailAddress = model.Email,
            TruckId = model.TruckId,
            DriversLicence = model.IdCard,
            PassportFile = model.Picture,
            Country = model.Country

        };

        // Check if TruckOwnerId is provided
        if (!string.IsNullOrEmpty(model.TruckOwnerId))
        {
            var truckOwner = await _context.TruckOwners
                .FirstOrDefaultAsync(to => to.Id == model.TruckOwnerId);

            if (truckOwner == null)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Truck owner not found",
                    StatusCode = 400 // Not Found
                };
            }

            // Set the TruckOwner for the driver
            newDriver.TruckOwnerId = truckOwner.Id; // Associate TruckOwner with Driver
        }

        _context.Drivers.Add(newDriver);
        var password = HelperClass.GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newDriver.Name, newDriver.EmailAddress, newDriver.Phone, "driver", password, false);

        //TODO:: Email password to user
        if (res.StatusCode == 201)
        {
            var user = await _userManager.FindByEmailAsync(newDriver.EmailAddress);
            newDriver.UserId = user.Id;
            newDriver.User = user;
            var emailSubject = "Account Created";
            await _emailSender.SendEmailAsync(newDriver.EmailAddress, emailSubject, password);

            // **Save changes to database**
            await _context.SaveChangesAsync();
            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Driver created successfully",
                StatusCode = 201,
                Data = password
            };
        }

        return new ApiResponseModel<string>
        {
            IsSuccessful = false,
            Message = "An error occurred while creating the driver",
            StatusCode = 400, // Bad Request
        };
    }

    public async Task<ApiResponseModel<bool>> EditDriver(EditDriverRequestModel model)
    {
        var driver = await _context.Drivers.FindAsync(model.Id);

        if (driver == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Driver not found",
                StatusCode = 404
            };
        }

        driver.Name = model.Name;
        driver.Phone = model.Number;


        driver.PassportFile = model.ProfilePicture ?? driver.PassportFile;

        // Save changes to database
        _context.Drivers.Update(driver);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Driver updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> DeactivateDriver(string driverId)
    {
        var driver = await _context.Drivers.FindAsync(driverId);
        if (driver == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Driver not found",
                StatusCode = 404 // Not Found
            };
        }

        driver.IsActive = false;
        _context.Drivers.Update(driver);
        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Driver deactivated successfully",
            StatusCode = 200,
            Data = true
        };
    }
    public async Task<ApiResponseModel<List<AllDriverResponseModel>>> GetAllDrivers()
    {
        var drivers = await _context.Drivers.ToListAsync();

        var driverResponseModels = _mapper.Map<List<AllDriverResponseModel>>(drivers);

        return new ApiResponseModel<List<AllDriverResponseModel>>
        {
            IsSuccessful = true,
            Message = "Drivers retrieved successfully",
            StatusCode = 200,
            Data = driverResponseModels
        };
    }

    public async Task<DriverResponseModel> GetDriverById(string id)
    {
        var driver = await _context.Drivers
            .Where(x => x.Id == id)
             .Include(e => e.User)
            .Include(d => d.Truck)
            .Include(d => d.BankAccounts)
            .Include(d => d.TermsAcceptanceRecords)
            .FirstOrDefaultAsync();

        if (driver == null)
        {
            // Return null (or throw an exception, depending on how you prefer to handle "not found" in your service)
            return null;
        }

        var driverToReturn = _mapper.Map<DriverResponseModel>(driver);
        driverToReturn.HasAcceptedTerms = driver.TermsAcceptanceRecords.Any(r => r.TermsVersion == "2025"); // Current version

        if (driver.Truck != null)
        {
            driverToReturn.Truck = _mapper.Map<AllTruckResponseModel>(driver.Truck);
        }

        return driverToReturn;
    }

    public async Task<ApiResponseModel<IEnumerable<AllDriverResponseModel>>> SearchDrivers(string searchWords)
    {
        IQueryable<Driver> query = _context.Drivers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " &&
            searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.Name.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var drivers = await query.ToListAsync();

        if (!drivers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllDriverResponseModel>>
            {
                Data = new List<AllDriverResponseModel> { },
                IsSuccessful = false,
                Message = "No driver found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllDriverResponseModel>>(drivers);

        return new ApiResponseModel<IEnumerable<AllDriverResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Drivers successfully retrieved",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<DriverProfileResponseModel>> GetDriverProfileById(string driverId)
    {
        var driver = await _context.Drivers
            .Include(e => e.User)
            .Include(d => d.Truck)
            .Include(d => d.TermsAcceptanceRecords)
            .FirstOrDefaultAsync(d => d.UserId == driverId);

        if (driver == null)
        {
            return new ApiResponseModel<DriverProfileResponseModel>
            {
                IsSuccessful = false,
                Message = "Driver not found",
                StatusCode = 404
            };
        }

        var mappedDriver = _mapper.Map<DriverProfileResponseModel>(driver);
        mappedDriver.HasAcceptedTerms = driver.TermsAcceptanceRecords.Any(r => r.TermsVersion == "2025"); // Current version
        return new ApiResponseModel<DriverProfileResponseModel>
        {
            IsSuccessful = true,
            Data = mappedDriver,
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<OrderCountByDriver>> GetOrderCountByDriver(string driverId)
    {
        var today = DateTime.Now;
        // var startOfWeek = today.StartOfWeek(DayOfWeek.Monday);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var orders = await _context.Orders
            .Include(r => r.Truck)
            .ThenInclude(e => e.Driver)
            .Where(o => o.Truck.DriverId == driverId && o.OrderStatus == OrderStatus.Delivered)
            .ToListAsync();

        // Handle the case where no orders are found
        if (!orders.Any())
        {
            return new ApiResponseModel<OrderCountByDriver>
            {
                IsSuccessful = true, // You might want to change this to false
                Message = "No completed orders found for this driver",
                Data = new OrderCountByDriver
                {
                    week = 0,
                    month = 0
                },
                StatusCode = 200
            };
        }

        var completedThisWeek = orders.Count();
        var completedThisMonth = orders.Count();

        return new ApiResponseModel<OrderCountByDriver>
        {
            IsSuccessful = true,
            Data =
            {
                week = completedThisWeek,
                month = completedThisMonth
            },
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<List<AllOrderResponseModel>>> GetOrderAssignedToDriver(string driverId)
    {
        var orders = await _context.Orders
            .Include(a => a.Business)
            .Include(b => b.Routes)
            .Include(c => c.Customer)
            .Include(r => r.Truck)
            .ThenInclude(e => e.Driver)
            .Where(o => o.Truck.Driver.Id == driverId)
            .ToListAsync();

        // Handle the case where no orders are found
        if (!orders.Any())
        {
            return new ApiResponseModel<List<AllOrderResponseModel>>
            {
                IsSuccessful = true, // You might want to change this to false
                Message = "No orders found for this driver",
                StatusCode = 200
            };
        }

        var mappedOrders = _mapper.Map<List<AllOrderResponseModel>>(orders);
        return new ApiResponseModel<List<AllOrderResponseModel>>
        {
            IsSuccessful = true,
            Data = mappedOrders,
            StatusCode = 200
        };
    }
    public async Task<ApiResponseModel<string>> CreateDriverAccount(CreateDriverRequestModel model)
    {
        // Validate input fields
        var validationErrors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(model.Name))
            validationErrors.Add("Name is required");
        else if (model.Name.Length < 2 || model.Name.Length > 100)
            validationErrors.Add("Name must be between 2 and 100 characters");
            
        if (string.IsNullOrWhiteSpace(model.Email))
            validationErrors.Add("Email is required");
        else if (!IsValidEmail(model.Email))
            validationErrors.Add("Invalid email format");
        else if (model.Email.Length > 256)
            validationErrors.Add("Email cannot exceed 256 characters");
            
        if (string.IsNullOrWhiteSpace(model.Number))
            validationErrors.Add("Phone number is required");
        else if (model.Number.Length < 10 || model.Number.Length > 20)
            validationErrors.Add("Phone number must be between 10 and 20 characters");
            
        if (string.IsNullOrWhiteSpace(model.Country))
            validationErrors.Add("Country is required");
        else if (model.Country.Length < 2 || model.Country.Length > 50)
            validationErrors.Add("Country must be between 2 and 50 characters");
            
        if (!string.IsNullOrWhiteSpace(model.address) && model.address.Length > 500)
            validationErrors.Add("Address cannot exceed 500 characters");
            
        if (string.IsNullOrWhiteSpace(model.password))
            validationErrors.Add("Password is required");
        else if (model.password.Length < 5)
            validationErrors.Add("Password must be at least 5 characters");
        
        if (validationErrors.Any())
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = string.Join("; ", validationErrors),
                StatusCode = 400,
                Data = null
            };
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Check for existing driver with same email or phone
            var existingDriver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.EmailAddress == model.Email || d.Phone == model.Number);

            if (existingDriver != null)
            {
                if (existingDriver.EmailAddress == model.Email)
                {
                    return new ApiResponseModel<string>
                    {
                        IsSuccessful = false,
                        Message = "Email address already exists",
                        StatusCode = 400
                    };
                }
                else
                {
                    return new ApiResponseModel<string>
                    {
                        IsSuccessful = false,
                        Message = "Phone number already exists",
                        StatusCode = 400
                    };
                }
            }

            // Create the authentication user first
            var authResult = await _authService.AddNewUserAsync(
                model.Name, 
                model.Email, 
                model.Number,
                "driver", 
                model.password, 
                true);

            if (authResult.StatusCode != 201)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = authResult.Message ?? "Failed to create user account",
                    StatusCode = authResult.StatusCode
                };
            }

            // Get the created user
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "User was created but could not be retrieved",
                    StatusCode = 500
                };
            }

            // Create the driver record
            var newDriver = new Driver
            {
                Name = model.Name,
                Phone = model.Number,
                EmailAddress = model.Email,
                Country = model.Country,
                OnboardingStatus = DriverOnboardingStatus.OboardingPending,
                UserId = user.Id,
                User = user
            };

            _context.Drivers.Add(newDriver);
            await _context.SaveChangesAsync();

            // Generate email confirmation token
            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string confirmationLink = $"https://trucki.co/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

            // Send welcome email
            await _emailSender.SendWelcomeEmailAsync(
                newDriver.EmailAddress,
                newDriver.Name,
                "driver",
                confirmationLink);

            await transaction.CommitAsync();

            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Driver account created successfully",
                StatusCode = 201,
                Data = newDriver.Id
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            // Log the exception (you might want to inject a logger)
            _logger.LogError(ex, "Error creating driver account for email: {Email}", model.Email);
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "An error occurred while creating the driver account",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponseModel<List<AllDriverResponseModel>>> GetDriversByTruckOwnerId(string truckOwnerId)
    {
        // Retrieve drivers that are associated with the given TruckOwnerId
        var drivers = await _context.Drivers
            .Where(d => d.TruckOwnerId == truckOwnerId)
            .ToListAsync();

        // If no drivers found, return a 404 response
        if (drivers == null || !drivers.Any())
        {
            return new ApiResponseModel<List<AllDriverResponseModel>>
            {
                IsSuccessful = false,
                Message = "No drivers found for this truck owner",
                StatusCode = 404,
                Data = new List<AllDriverResponseModel>()
            };
        }

        // Map drivers to the response model
        var driverResponseModels = _mapper.Map<List<AllDriverResponseModel>>(drivers);

        return new ApiResponseModel<List<AllDriverResponseModel>>
        {
            IsSuccessful = true,
            Message = "Drivers retrieved successfully",
            StatusCode = 200,
            Data = driverResponseModels
        };
    }
    public async Task<ApiResponseModel<bool>> AcceptTermsAndConditions(AcceptTermsRequestModel model)
    {
        try
        {
            var driver = await _context.Drivers
                .Include(d => d.TermsAcceptanceRecords)
                .FirstOrDefaultAsync(d => d.Id == model.DriverId);

            if (driver == null)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Driver not found",
                    StatusCode = 404
                };
            }

            // Create a new acceptance record
            var termsRecord = new TermsAcceptanceRecord
            {
                DriverId = driver.Id,
                TermsVersion = model.TermsVersion,
                AcceptedAt = model.AcceptedAt,
                AcceptedFromIp = model.IpAddress, // If you're collecting this
                DeviceInfo = model.DeviceInfo    // If you're collecting this
            };

            // Add to the database
            _context.TermsAcceptanceRecords.Add(termsRecord);
            await _context.SaveChangesAsync();

            return new ApiResponseModel<bool>
            {
                IsSuccessful = true,
                Message = "Terms and conditions accepted successfully",
                StatusCode = 200,
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = $"An error occurred: {ex.Message}",
                StatusCode = 500
            };
        }
    }
    public async Task<ApiResponseModel<bool>> HasAcceptedLatestTerms(string driverId)
    {
        try
        {
            var latestAcceptance = await _context.TermsAcceptanceRecords
                .Where(t => t.DriverId == driverId)
                .OrderByDescending(t => t.AcceptedAt)
                .FirstOrDefaultAsync();

            bool hasAccepted = latestAcceptance != null && latestAcceptance.TermsVersion == "2025"; // Current version

            return new ApiResponseModel<bool>
            {
                IsSuccessful = true,
                Message = hasAccepted ? "Driver has accepted latest terms" : "Driver has not accepted latest terms",
                StatusCode = 200,
                Data = hasAccepted
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = $"An error occurred: {ex.Message}",
                StatusCode = 500
            };
        }
    }
    public async Task<ApiResponseModel<List<TermsAcceptanceRecordDto>>> GetTermsAcceptanceHistory(string driverId)
    {
        try
        {
            var history = await _context.TermsAcceptanceRecords
                .Include(t => t.Driver) // Include driver for name info
                .Where(t => t.DriverId == driverId)
                .OrderByDescending(t => t.AcceptedAt)
                .Select(t => new TermsAcceptanceRecordDto
                {
                    Id = t.Id,
                    DriverId = t.DriverId,
                    DriverName = t.Driver.Name,
                    TermsVersion = t.TermsVersion,
                    AcceptedAt = t.AcceptedAt,
                    AcceptedFromIp = t.AcceptedFromIp,
                    DeviceInfo = t.DeviceInfo
                })
                .ToListAsync();

            return new ApiResponseModel<List<TermsAcceptanceRecordDto>>
            {
                IsSuccessful = true,
                Message = "Terms acceptance history retrieved successfully",
                StatusCode = 200,
                Data = history
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<List<TermsAcceptanceRecordDto>>
            {
                IsSuccessful = false,
                Message = $"An error occurred: {ex.Message}",
                StatusCode = 500
            };
        }
    }
    public async Task<ApiResponseModel<bool>> UpdateDriverProfilePhoto(UpdateDriverProfilePhotoRequestModel model)
    {
        var driver = await _context.Drivers.FindAsync(model.DriverId);

        if (driver == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Driver not found",
                StatusCode = 404
            };
        }

        driver.PassportFile = model.ProfilePhotoUrl;

        // Save changes to database
        _context.Drivers.Update(driver);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Profile photo updated successfully",
            StatusCode = 200,
            Data = true
        };
    }
    public async Task<ApiResponseModel<bool>> UpdateDriverOnboardingStatus(string driverId, DriverOnboardingStatus status)
    {
        try
        {
            var driver = await _context.Drivers.FindAsync(driverId);

            if (driver == null)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Driver not found",
                    StatusCode = 404
                };
            }

            // Update the onboarding status
            driver.OnboardingStatus = status;

            _context.Drivers.Update(driver);
            await _context.SaveChangesAsync();

            return new ApiResponseModel<bool>
            {
                IsSuccessful = true,
                Message = $"Driver onboarding status updated successfully to {status}",
                StatusCode = 200,
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = $"An error occurred while updating driver onboarding status: {ex.Message}",
                StatusCode = 500
            };
        }
    }
    public async Task<ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>> GetAllDriversPaginated(GetAllDriversRequestModel request)
{
    try
    {
        // Start with base query
        IQueryable<Driver> driversQuery = _context.Drivers.Include(d => d.User);

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            driversQuery = driversQuery.Where(d => d.Name.ToLower().Contains(request.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            driversQuery = driversQuery.Where(d => d.EmailAddress.ToLower().Contains(request.Email.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            driversQuery = driversQuery.Where(d => d.Country.ToLower() == request.Country.ToLower());
        }

        // Apply sorting
        driversQuery = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? driversQuery.OrderByDescending(d => d.Name)
                : driversQuery.OrderBy(d => d.Name),
            "email" => request.SortDescending
                ? driversQuery.OrderByDescending(d => d.EmailAddress)
                : driversQuery.OrderBy(d => d.EmailAddress),
            "country" => request.SortDescending
                ? driversQuery.OrderByDescending(d => d.Country)
                : driversQuery.OrderBy(d => d.Country),
            "createdat" => request.SortDescending
                ? driversQuery.OrderByDescending(d => d.CreatedAt)
                : driversQuery.OrderBy(d => d.CreatedAt),
            _ => request.SortDescending
                ? driversQuery.OrderByDescending(d => d.CreatedAt)
                : driversQuery.OrderBy(d => d.CreatedAt)
        };

        // Get total count before pagination
        var totalCount = await driversQuery.CountAsync();

        // Apply pagination
        var drivers = await driversQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Map to response models
        var driverResponseModels = _mapper.Map<List<AllDriverResponseModel>>(drivers);

        // Create paginated result
        var paginatedResult = new PaginatedListDto<AllDriverResponseModel>
        {
            Data = driverResponseModels,
            MetaData = new PageMeta
            {
                Page = request.PageNumber,
                PerPage = request.PageSize,
                Total = totalCount,
                TotalPages = totalCount % request.PageSize == 0 
                    ? totalCount / request.PageSize 
                    : totalCount / request.PageSize + 1
            }
        };

        return new ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>
        {
            Data = paginatedResult,
            IsSuccessful = true,
            Message = totalCount > 0 
                ? $"Successfully retrieved {drivers.Count} of {totalCount} drivers"
                : "No drivers found matching the criteria",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        return new ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>
        {
            Data = null,
            IsSuccessful = false,
            Message = $"An error occurred while retrieving drivers: {ex.Message}",
            StatusCode = 500
        };
    }
}

public async Task<ApiResponseModel<AdminDriverSummaryResponseModel>> GetAdminDriversSummary()
{
    try
    {
        var drivers = await _context.Drivers.ToListAsync();

        var summary = new AdminDriverSummaryResponseModel
        {
            TotalDrivers = drivers.Count,
            TotalUSDrivers = drivers.Count(d => d.Country?.ToUpper() == "US" || d.Country?.ToUpper() == "USA"),
            TotalNigerianDrivers = drivers.Count(d => d.Country?.ToUpper() == "NG" || d.Country?.ToUpper() == "NIGERIA"),
            ActiveDrivers = drivers.Count(d => d.IsActive),
            InactiveDrivers = drivers.Count(d => !d.IsActive)
        };

        // Group drivers by country
        summary.DriversByCountry = drivers
            .GroupBy(d => d.Country?.ToUpper() ?? "UNKNOWN")
            .ToDictionary(g => g.Key, g => g.Count());

        return new ApiResponseModel<AdminDriverSummaryResponseModel>
        {
            Data = summary,
            IsSuccessful = true,
            Message = "Driver summary retrieved successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        return new ApiResponseModel<AdminDriverSummaryResponseModel>
        {
            Data = null,
            IsSuccessful = false,
            Message = $"An error occurred while retrieving driver summary: {ex.Message}",
            StatusCode = 500
        };
    }
}

public async Task<ApiResponseModel<bool>> UpdateDotNumber(UpdateDotNumberRequestModel model)
{
    try
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == model.DriverId);

        if (driver == null)
        {
            return new ApiResponseModel<bool>
            {
                Data = false,
                IsSuccessful = false,
                Message = "Driver not found",
                StatusCode = 404
            };
        }

        // Validate DOT number based on driver's country
        if (driver.Country == "US")
        {
            if (string.IsNullOrEmpty(model.DotNumber))
            {
                return new ApiResponseModel<bool>
                {
                    Data = false,
                    IsSuccessful = false,
                    Message = "DOT number is required for US drivers",
                    StatusCode = 400
                };
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(model.DotNumber, @"^\d{7,12}$"))
            {
                return new ApiResponseModel<bool>
                {
                    Data = false,
                    IsSuccessful = false,
                    Message = "DOT number must be 7-12 digits for US drivers",
                    StatusCode = 400
                };
            }
        }

        // Check if DOT number already exists for another driver
        var existingDotNumber = await _context.Drivers
            .FirstOrDefaultAsync(d => d.DotNumber == model.DotNumber && d.Id != model.DriverId);
        
        if (existingDotNumber != null)
        {
            return new ApiResponseModel<bool>
            {
                Data = false,
                IsSuccessful = false,
                Message = "This DOT number is already in use by another driver",
                StatusCode = 400
            };
        }

        driver.DotNumber = model.DotNumber;
        driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            Data = true,
            IsSuccessful = true,
            Message = "DOT number updated successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        return new ApiResponseModel<bool>
        {
            Data = false,
            IsSuccessful = false,
            Message = $"An error occurred while updating DOT number: {ex.Message}",
            StatusCode = 500
        };
    }
}

    public async Task<ApiResponseModel<AdminDriverDetailsResponseModel>> GetDriverDetailsForAdmin(string driverId)
    {
        try
        {
            var driver = await _context.Drivers
                .Where(d => d.Id == driverId)
                .Include(d => d.User)
                .Include(d => d.Truck)
                    .ThenInclude(t => t.TruckOwner)
                .Include(d => d.BankAccounts)
                .Include(d => d.TermsAcceptanceRecords)
                .Include(d => d.DriverDocuments)
                    .ThenInclude(dd => dd.DocumentType)
                .FirstOrDefaultAsync();

            if (driver == null)
            {
                return new ApiResponseModel<AdminDriverDetailsResponseModel>
                {
                    IsSuccessful = false,
                    Message = "Driver not found",
                    StatusCode = 404
                };
            }

            // Get all required document types for this driver's country
            var requiredDocumentTypes = await _context.DocumentTypes
                .Where(dt => dt.Country == driver.Country && dt.EntityType == "Driver" && dt.IsRequired)
                .ToListAsync();

            // Build document status list
            var documentStatuses = new List<DriverDocumentStatusDto>();
            foreach (var docType in requiredDocumentTypes)
            {
                var driverDoc = driver.DriverDocuments.FirstOrDefault(dd => dd.DocumentTypeId == docType.Id);

                documentStatuses.Add(new DriverDocumentStatusDto
                {
                    DocumentTypeId = docType.Id,
                    DocumentTypeName = docType.Name,
                    IsRequired = docType.IsRequired,
                    IsUploaded = driverDoc != null,
                    ApprovalStatus = driverDoc?.ApprovalStatus ?? "NotUploaded",
                    FileUrl = driverDoc?.FileUrl ?? "",
                    RejectionReason = driverDoc?.RejectionReason ?? ""
                });
            }

            // Calculate document summary
            var documentSummary = new DocumentUploadSummary
            {
                TotalRequiredDocuments = requiredDocumentTypes.Count,
                UploadedDocuments = documentStatuses.Count(ds => ds.IsUploaded),
                ApprovedDocuments = documentStatuses.Count(ds => ds.ApprovalStatus == "Approved"),
                RejectedDocuments = documentStatuses.Count(ds => ds.ApprovalStatus == "Rejected"),
                PendingDocuments = documentStatuses.Count(ds => ds.ApprovalStatus == "Pending")
            };

            // Get terms acceptance history
            var termsHistory = driver.TermsAcceptanceRecords
                .OrderByDescending(t => t.AcceptedAt)
                .Select(t => new TermsAcceptanceRecordDto
                {
                    TermsVersion = t.TermsVersion,
                    AcceptedAt = t.AcceptedAt,
                    AcceptedFromIp = t.AcceptedFromIp,
                    DeviceInfo = t.DeviceInfo
                })
                .ToList();

            // Build truck info if truck exists
            AdminDriverTruckInfo? truckInfo = null;
            if (driver.Truck != null)
            {
                truckInfo = new AdminDriverTruckInfo
                {
                    TruckId = driver.Truck.Id,
                    PlateNumber = driver.Truck.PlateNumber,
                    TruckName = driver.Truck.TruckName,
                    TruckCapacity = driver.Truck.TruckCapacity,
                    TruckType = driver.Truck.TruckType,
                    ApprovalStatus = driver.Truck.ApprovalStatus,
                    TruckStatus = driver.Truck.TruckStatus,
                    IsDriverOwnedTruck = driver.Truck.IsDriverOwnedTruck,
                    CreatedAt = driver.Truck.CreatedAt,
                    ExternalTruckPictureUrl = driver.Truck.ExternalTruckPictureUrl,
                    CargoSpacePictureUrl = driver.Truck.CargoSpacePictureUrl,
                    Documents = driver.Truck.Documents ?? new List<string>(),
                    TruckLicenseExpiryDate = driver.Truck.TruckLicenseExpiryDate,
                    RoadWorthinessExpiryDate = driver.Truck.RoadWorthinessExpiryDate,
                    InsuranceExpiryDate = driver.Truck.InsuranceExpiryDate,
                    TruckOwnerId = driver.Truck.TruckOwnerId,
                    TruckOwnerName = driver.Truck.TruckOwner?.Name
                };
            }

            // Calculate onboarding progress
            var progress = new OnboardingProgressSummary
            {
                TermsAccepted = termsHistory.Any(t => t.TermsVersion == "2025"), // Current version
                ProfilePictureUploaded = !string.IsNullOrEmpty(driver.PassportFile),
                AllDocumentsUploaded = documentSummary.AllRequiredDocumentsUploaded,
                AllDocumentsApproved = documentSummary.AllDocumentsApproved,
                TruckAdded = driver.Truck != null,
                TruckApproved = driver.Truck?.ApprovalStatus == ApprovalStatus.Approved
            };

            // Build completed and pending steps lists
            progress.CompletedSteps = new List<string>();
            progress.PendingSteps = new List<string>();
            progress.RejectedItems = new List<string>();

            if (progress.TermsAccepted) progress.CompletedSteps.Add("Terms and Conditions Accepted");
            else progress.PendingSteps.Add("Accept Terms and Conditions");

            if (progress.ProfilePictureUploaded) progress.CompletedSteps.Add("Profile Picture Uploaded");
            else progress.PendingSteps.Add("Upload Profile Picture");

            if (progress.AllDocumentsUploaded) progress.CompletedSteps.Add("All Documents Uploaded");
            else progress.PendingSteps.Add("Upload Required Documents");

            if (progress.AllDocumentsApproved) progress.CompletedSteps.Add("All Documents Approved");
            else if (progress.AllDocumentsUploaded) progress.PendingSteps.Add("Document Approval");

            if (progress.TruckAdded) progress.CompletedSteps.Add("Truck Added");
            else progress.PendingSteps.Add("Add Truck");

            if (progress.TruckApproved) progress.CompletedSteps.Add("Truck Approved");
            else if (progress.TruckAdded)
            {
                if (driver.Truck?.ApprovalStatus == ApprovalStatus.NotApproved || driver.Truck?.ApprovalStatus == ApprovalStatus.Blocked)
                    progress.RejectedItems.Add("Truck approval rejected/blocked");
                else
                    progress.PendingSteps.Add("Truck Approval");
            }

            // Add rejected documents to rejected items
            foreach (var rejectedDoc in documentStatuses.Where(ds => ds.ApprovalStatus == "Rejected"))
            {
                progress.RejectedItems.Add($"{rejectedDoc.DocumentTypeName} document rejected");
            }

            progress.CompletedStepCount = progress.CompletedSteps.Count;
            progress.OverallStatus = progress.CanBeApproved ? "Ready for Approval" :
                                   progress.RejectedItems.Any() ? "Has Rejections" :
                                   "In Progress";

            // Build the response
            var response = new AdminDriverDetailsResponseModel
            {
                Id = driver.Id,
                Name = driver.Name,
                Phone = driver.Phone,
                EmailAddress = driver.EmailAddress,
                UserId = driver.UserId,
                DriversLicence = driver.DriversLicence,
                DotNumber = driver.DotNumber,
                Country = driver.Country,
                IsActive = driver.IsActive,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt,
                ProfilePictureUrl = driver.PassportFile,
                OnboardingStatus = driver.OnboardingStatus,
                HasAcceptedLatestTerms = progress.TermsAccepted,
                TermsAcceptanceHistory = termsHistory,
                LatestTermsAcceptedAt = termsHistory.FirstOrDefault()?.AcceptedAt,
                DocumentStatuses = documentStatuses,
                DocumentSummary = documentSummary,
                TruckInfo = truckInfo,
                StripeConnectAccountId = driver.StripeConnectAccountId,
                StripeAccountStatus = driver.StripeAccountStatus,
                CanReceivePayouts = driver.CanReceivePayouts,
                BankAccounts = _mapper.Map<ICollection<DriverBankAccountResponseModel>>(driver.BankAccounts),
                OnboardingProgress = progress
            };

            return new ApiResponseModel<AdminDriverDetailsResponseModel>
            {
                IsSuccessful = true,
                Message = "Driver details retrieved successfully",
                Data = response,
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving driver details for admin for driver {DriverId}", driverId);
            return new ApiResponseModel<AdminDriverDetailsResponseModel>
            {
                IsSuccessful = false,
                Message = "An error occurred while retrieving driver details",
                StatusCode = 500
            };
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}