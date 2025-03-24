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

    public DriverRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
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
            .Include(d => d.Truck)
            .FirstOrDefaultAsync();

        if (driver == null)
        {
            // Return null (or throw an exception, depending on how you prefer to handle "not found" in your service)
            return null;
        }

        var driverToReturn = _mapper.Map<DriverResponseModel>(driver);

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
        var existingManager = await _context.Drivers
            .FirstOrDefaultAsync(m => m.EmailAddress == model.Email || m.Phone == model.Number);

        if (existingManager != null)
        {
            if (existingManager.EmailAddress == model.Email)
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
            Country = model.Country,
            //PassportFile = "",
            //DriversLicence = ""
        };
        newDriver.OnboardingStatus = DriverOnboardingStatus.OboardingPending;
        _context.Drivers.Add(newDriver);
        var res = await _authService.AddNewUserAsync(newDriver.Name, newDriver.EmailAddress, newDriver.Phone,
            "driver", model.password, true);
        //TODO:: Email password to user
        if (res.StatusCode == 201)
        {
            var user = await _userManager.FindByEmailAsync(newDriver.EmailAddress);
            newDriver.UserId = user.Id;
            newDriver.User = user;
            var emailSubject = "Account Created";
            // await _emailSender.SendEmailAsync(newDriver.EmailAddress, emailSubject, password);
            // **Save changes to database**
            await _context.SaveChangesAsync();
            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string confirmationLink = $"https://trucki.co/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

            // Send welcome email
            await _emailSender.SendWelcomeEmailAsync(
                newDriver.EmailAddress,
                newDriver.Name,
                "driver",
                confirmationLink);
            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Driver created successfully",
                StatusCode = 201,
                Data = ""
            };
        }

        return new ApiResponseModel<string>
        {
            IsSuccessful = false,
            Message = "An error occurred while creating the manager",
            StatusCode = 400, // Bad Request
        };
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
}