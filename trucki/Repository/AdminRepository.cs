using AutoMapper;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class AdminRepository : IAdminRepository
{
    private readonly TruckiDBContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;

    public AdminRepository(TruckiDBContext appDbContext, IMapper mapper,IAuthService authService)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
    }

    public async Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel model)
    {
        var newBusiness = new Business
        {
            Name = model.Name,
            Ntons = model.Ntons,
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
        var newRoute = new Routes
        {
            Name = model.Name,
            FromRoute = model.FromRoute,
            ToRoute = model.ToRoute,
            Price = model.Price,
            IsActive = model.IsActive
        };

        // Add the route to the business
        business.Routes ??= new List<Routes>();
        business.Routes.Add(newRoute);

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
        business.Ntons = model.Ntons;
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
        var route = await _context.RoutesEnumerable.FindAsync(model.Id);

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
        route.ToRoute = model.ToRoute;
        route.Price = model.Price;
        route.IsActive = model.IsActive;

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
        var password = GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newManager.Name, newManager.EmailAddress,
            newManager.ManagerType == 0 ? "manager" : "finance manager", password);
        //TODO:: Email password to user
        if (res.StatusCode == 201)
        {
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
    public static string GenerateRandomPassword(int length = 6)
    {
        // Define character sets for password generation
        const string lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        const string uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string symbols = "!@#$%^&*()-_=+[]{};':\"\\|,<.>/?";

        // Combine all character sets (adjust as needed)
        string chars = lowercaseLetters + uppercaseLetters + digits + symbols;

        // Create a random number generator
        var random = new Random();

        // Generate a random password of the specified length
        var password = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return password;
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
        var manager = await _context.Managers.FindAsync(managerId);
        if (manager == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Manager not found",
                StatusCode = 404 // Not Found
            };
        }

        manager.IsActive = false;
        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Manager deactivated successfully",
            StatusCode = 200,
            Data = true
        };
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
            PassportFile = "",
            DriversLicence = ""
            
        };
        _context.Drivers.Add(newDriver);
        var password = GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newDriver.Name, newDriver.EmailAddress,
            "driver", password);
        //TODO:: Email password to user
        if (res.StatusCode == 201)
        {
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
}