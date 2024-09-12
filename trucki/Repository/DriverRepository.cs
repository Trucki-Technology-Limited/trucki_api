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

public class DriverRepository:IDriverRepository
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
            TruckId = model.TruckId,
            //PassportFile = "",
            //DriversLicence = ""
        };

        // Id card
        newDriver.DriversLicence = model.IdCard;
        
        // Profile picture
        newDriver.PassportFile = model.Picture;


        _context.Drivers.Add(newDriver);
        var password = HelperClass.GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newDriver.Name, newDriver.EmailAddress,newDriver.Phone,
            "driver", password);
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
            Message = "An error occurred while creating the manager",
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

    public async Task<ApiResponseModel<DriverResponseModel>> GetDriverById(string id)
    {
        var driver = await _context.Drivers.Where(x => x.Id == id).Include(d => d.Truck).FirstOrDefaultAsync();

        if (driver == null)
            return new ApiResponseModel<DriverResponseModel>
            {
                Data = new DriverResponseModel { }, IsSuccessful = false, Message = "Driver not found",
                StatusCode = 404
            };

        var driverToReturn = _mapper.Map<DriverResponseModel>(driver);
        if (driver.Truck != null)
        {
            driverToReturn.Truck = _mapper.Map<AllTruckResponseModel>(driver.Truck); 
        }

        return new ApiResponseModel<DriverResponseModel>
        {
            IsSuccessful = true,
            Message = "Driver retrieved successfully",
            StatusCode = 200,
            Data = driverToReturn
        };
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
                Data =  new OrderCountByDriver
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
}