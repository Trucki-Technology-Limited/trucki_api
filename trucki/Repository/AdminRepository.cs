using AutoMapper;
using CloudinaryDotNet.Actions;
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
    private readonly IUploadService _uploadService;

    public AdminRepository(TruckiDBContext appDbContext, IMapper mapper,IAuthService authService,IUploadService uploadService)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
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
        foreach (var i in model.Routes) {
            var newRoute = new Routes
            {
                Name = i.Name,
                FromRoute = i.FromRoute,
                ToRoute = i.ToRoute,
                Price = i.Price,
                IsActive = i.IsActive,
                Gtv = i.Gtv
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

    public async Task<ApiResponseModel<AllManagerResponseModel>> GetManagerById(string id)
    {
        var manager = await _context.Managers.Where(x => x.Id == id).Include(b => b.Company).FirstOrDefaultAsync();

        if (manager == null)
            return new ApiResponseModel<AllManagerResponseModel> { Data = new AllManagerResponseModel { }, IsSuccessful = false, Message = "No manager found", StatusCode = 404 };

        var managerToReturn = _mapper.Map<AllManagerResponseModel>(manager);


        return new ApiResponseModel<AllManagerResponseModel>
        {
            IsSuccessful = true,
            Message = "Manager retrieved successfully",
            StatusCode = 200,
            Data = managerToReturn
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
            //PassportFile = "",
            //DriversLicence = ""
            
        };

        var imagePath = "";
        if (model.IdCard != null && model.IdCard.Length > 0)
        {
            // Save Id card 
            imagePath = await _uploadService.UploadFile(model.IdCard, $"{newDriver.Name}userIdCard");
        }

        // Id card
        newDriver.DriversLicence = imagePath;

        var profilePicPath = "";
        if(model.Picture != null && model.Picture.Length > 0)
        {
            // save profile picture
            profilePicPath = await _uploadService.UploadFile(model.Picture, $"{newDriver.Name}userProfilePicture");
        }


        // Profile picture
        newDriver.PassportFile = profilePicPath;


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

        var imagePath = "";
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            imagePath = await _uploadService.UploadFile(model.ProfilePicture, $"{driver.Name}userIdCard");
        }

        driver.PassportFile = imagePath;

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

    public async Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model)
    {
        // Create a new TruckiOwner instance
        var newOwner = new TruckOwner
        {
            Name = model.Name,
            EmailAddress = model.EmailAddress,
            Phone = model.Phone,
            Address = model.Address
        };

        // Add owner to context and save changes
        var imagePath = "";
        if (model.IdCard != null && model.IdCard.Length > 0)
        {
            // Save image (locally, to database, or external storage)
            imagePath =await _uploadService.UploadFile(model.IdCard,$"{newOwner.Name}userIdCard");
        }

        var profilePicPath = "";
        if(model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            profilePicPath = await _uploadService.UploadFile(model.ProfilePicture, $"{newOwner.Name}userProfilePicture");
        }
      
        newOwner.IdCardUrl = imagePath;
        newOwner.ProfilePictureUrl = profilePicPath;
        _context.TruckOwners.Add(newOwner);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Truck owner created successfully",
            StatusCode = 201,
            Data = true
        };
    }
    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id)
    {
        // Fetch owner from database
        var owner = await _context.TruckOwners.FindAsync(id);

        // Check if owner exists
        if (owner == null)
        {
            return new ApiResponseModel<TruckOwnerResponseModel>
            {
                IsSuccessful = false,
                Message = "Truck owner not found",
                StatusCode = 404
            };
        }
        var result = _mapper.Map<TruckOwnerResponseModel>(owner);
        return new ApiResponseModel<TruckOwnerResponseModel>
        {
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Success",
            Data = result
        };
    }
    public async Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model)
    {
        // Find the owner to update
        var owner = await _context.TruckOwners.FindAsync(model.Id);

        // Check if owner exists
        if (owner == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Truck owner not found",
                StatusCode = 404
            };
        }

        // Update properties directly on the existing instance
        owner.Name = model.Name;
        owner.EmailAddress = model.EmailAddress;
        owner.Phone = model.Phone;
        owner.Address = model.Address;

        // Save changes to database
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Truck owner updated successfully",
            StatusCode = 200,
            Data = true
        };
    }
    public async Task<ApiResponseModel<bool>> DeleteTruckOwner(string id)
    {
        // Find the owner to delete
        var owner = await _context.TruckOwners.FindAsync(id);

        // Check if owner exists
        if (owner == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Truck owner not found",
                StatusCode = 404
            };
        }

        // Remove owner from context and save changes
        _context.TruckOwners.Remove(owner);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Truck owner deleted successfully",
            StatusCode = 200,
            Data = true
        };
    }
    public async Task<ApiResponseModel<List<TruckOwnerResponseModel>>> GetAllTruckOwners()
    {
        // Fetch all truck owners from the database
        var owners = await _context.TruckOwners.ToListAsync();
        var result = _mapper.Map<List<TruckOwnerResponseModel>>(owners);
        return new ApiResponseModel<List<TruckOwnerResponseModel>>
        {
            IsSuccessful = true,
            StatusCode = 200,
            Data = result
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

    public async Task<ApiResponseModel<AllDriverResponseModel>> GetDriverById(string id)
    {
        var driver = await _context.Drivers.Where(x => x.Id == id).FirstOrDefaultAsync();

        if (driver == null)
            return new ApiResponseModel<AllDriverResponseModel> { Data = new AllDriverResponseModel { }, IsSuccessful = false, Message = "No manager found", StatusCode = 404 };

        var driverToReturn = _mapper.Map<AllDriverResponseModel>(driver);


        return new ApiResponseModel<AllDriverResponseModel>
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

        if(!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
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

    public async Task<ApiResponseModel<IEnumerable<AllManagerResponseModel>>> SearchManagers(string searchWords)
    {
        IQueryable<Manager> query = _context.Managers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
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

    public async Task<ApiResponseModel<IEnumerable<AllBusinessResponseModel>>> SearchBusinesses(string searchWords)
    {
        IQueryable<Business> query = _context.Businesses;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
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

    public async Task<ApiResponseModel<string>> AddOfficer(AddOfficerRequestModel model)
    {
        var existingOfficer = await _context.Officers.FirstOrDefaultAsync(m => m.EmailAddress == model.EmailAddress || m.PhoneNumber == model.PhoneNumber);
        if(existingOfficer != null)
        {
            if(existingOfficer.EmailAddress == model.EmailAddress)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Email address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
            else if(existingOfficer.PhoneNumber == model.PhoneNumber)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Phone Number address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
        }

        var newOfficer = new Officer
        {
            OfficerName = model.OfficerName,
            PhoneNumber = model.PhoneNumber,
            EmailAddress = model.EmailAddress,
            OfficerType = model.OfficerType, 
            CompanyId = model.CompanyId
        };

        _context.Officers.Add(newOfficer);

        var password = GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newOfficer.OfficerName,
            newOfficer.EmailAddress, newOfficer.OfficerType == 0 ? "field officer" : "safety officer", password);
        if(res.StatusCode == 201)
        {
            await _context.SaveChangesAsync();

            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Officer Succesfully created",
                StatusCode = 201,
                Data = password
            };
        }
        return new ApiResponseModel<string>
        {
            IsSuccessful = false,
            Message = "An error occurred while creating officer",
            StatusCode = 400, // Bad Request
        };

    }

    public async Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllFieldOfficers(int page, int size)
    {
        var fieldOfficers = await _context.Officers.Where(x => x.OfficerType == OfficerType.FieldOfficer)
            //.Skip(((page - 1) * size))
            //.Take(size)
            .ToListAsync();

        var totalItems =  fieldOfficers.Count();

        var data = _mapper.Map<IEnumerable<AllOfficerResponseModel>>(fieldOfficers);
        var paginatedList = PagedList<AllOfficerResponseModel>.Paginates(data, page, size);

        return new ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>
        {
            IsSuccessful = true,
            Message = "Officers retrieved successfully",
            StatusCode = 200,
            Data = paginatedList
        };
    }

    public async Task<ApiResponseModel<bool>> EditOfficer(EditOfficerRequestModel model)
    {
        var officer = await _context.Officers.FindAsync(model.OfficerId);
        if (officer == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Officer not found",
                StatusCode = 404 // Not Found
            };
        }

        officer.OfficerName = model.OfficerName;
        officer.PhoneNumber = model.PhoneNumber;
        officer.EmailAddress = model.EmailAddress;
        officer.OfficerType = model.OfficerType;
        officer.CompanyId = model.CompanyId;
        officer.UpdatedAt = DateTime.Now.ToUniversalTime();
        // Update company list logic can be added here similar to AddManager

        _context.Officers.Update(officer);
        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Officer details updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model)
    {
        var existingTruck = await _context.Trucks.Where(x => x.PlateNumber == model.PlateNumber).FirstOrDefaultAsync();
        if(existingTruck != null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Truck already exist",
                StatusCode = 400
            };
        }

        var truckOwner = await _context.TruckOwners.FindAsync(model.TruckOwnerId);

        if (truckOwner == null)
        {
            // Handle error - Truck owner not found
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Truck owner not found",
                StatusCode = 400
            };
        }

        var newTruck = new Truck
        {
            //CertOfOwnerShip = model.CertOfOwnerShip,
            PlateNumber = model.PlateNumber,
            TruckCapacity = model.TruckCapacity,
            DriverId = model.DriverId,
            //Capacity = model.Capacity,
            TruckOwnerId = model.TruckOwnerId,
            TruckOwner = truckOwner,
            TruckType = model.TruckType,
            TruckLicenseExpiryDate = model.TruckLicenseExpiryDate,
            RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate,
            InsuranceExpiryDate = model.InsuranceExpiryDate
        };

        List<string> documents = new List<string>();

        if (model.Documents != null)
        {
            foreach (var document in model.Documents)
            {
                documents.Add(await _uploadService.UploadFile(document, $"{newTruck.PlateNumber}"));
            }
        }

        newTruck.Documents = documents;

        _context.Trucks.Add(newTruck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Truck added successfully",
            StatusCode = 200,
            Data = newTruck.Id 
        };

    }

    public async Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model)
    {
        var truck = await _context.Trucks.Where(x => x.Id == model.TruckId).FirstOrDefaultAsync();
        if(truck == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Truck not found",
                StatusCode = 404
            };
        }
        var truckOwner = await _context.TruckOwners.FindAsync(model.TruckOwnerId);

        if (truckOwner == null)
        {
            // Handle error - Truck owner not found
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Truck owner not found",
                StatusCode = 400
            };
        }
        //truck.CertOfOwnerShip = model.CertOfOwnerShip;
        truck.PlateNumber = model.PlateNumber;
        truck.TruckCapacity = model.TruckCapacity;
        truck.DriverId = model.DriverId;
        //truck.Capacity = model.Capacity;
        truck.TruckOwnerId = model.TruckOwnerId;
        truck.TruckOwner = truckOwner;
        truck.TruckType = model.TruckType;
        truck.TruckLicenseExpiryDate = model.TruckLicenseExpiryDate;
        truck.RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate;
        truck.InsuranceExpiryDate = model.InsuranceExpiryDate;

        // Upload documents
        if (model.Documents != null)
        {
            foreach (var document in model.Documents)
            {
                var uploadedDocument = await _uploadService.UploadFile(document, $"{truck.PlateNumber}");
                truck.Documents.Add(uploadedDocument);
            }
        }

        // Save changes to the database
        _context.Trucks.Update(truck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Truck updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<string>> DeleteTruck(string truckId)
    {
        var truck = await _context.Trucks.Where(x => x.Id == truckId).FirstOrDefaultAsync();
        if(truck == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Truck not found",
                StatusCode = 404
            };
        }
        _context.Trucks.Remove(truck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Succesfully deleted truck",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId)
    {
        var truck = await _context.Trucks.Where(x => x.Id == truckId).FirstOrDefaultAsync();
        if(truck == null)
        {
            return new ApiResponseModel<AllTruckResponseModel>
            {
                IsSuccessful = false,
                Message = "Truck not found",
                StatusCode = 404,
                Data = new AllTruckResponseModel { }
            };
        }

        var driver = await _context.Drivers.Where(x => x.Id == truck.DriverId).FirstOrDefaultAsync();

        var truckOwner = await _context.TruckOwners.Where(x => x.Id == truck.TruckOwnerId).FirstOrDefaultAsync();

        var truckToReturn = new AllTruckResponseModel
        {
            Id = truck.Id,
            Documents = truck.Documents,
            //CertOfOwnerShip = truck.CertOfOwnerShip,
            PlateNumber = truck.PlateNumber,
            TruckCapacity = truck.TruckCapacity,
            DriverId = truck.DriverId,
            DriverName = driver?.Name,
            //Capacity = truck.Capacity,
            TruckOwnerId = truck.TruckOwnerId,
            TruckOwnerName = truckOwner.Name,
            TruckType = truck.TruckType,
            TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
            RoadWorthinessExpiryDate = truck.RoadWorthinessExpiryDate,
            InsuranceExpiryDate = truck.InsuranceExpiryDate
        };

        return new ApiResponseModel<AllTruckResponseModel>
        {
            IsSuccessful = true,
            Message = "Truck found",
            StatusCode = 200,
            Data = truckToReturn
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords)
    {
        IQueryable<Truck> query = _context.Trucks;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.PlateNumber.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var trucks = await query.ToListAsync();

        if (!trucks.Any())
        {
            return new ApiResponseModel<IEnumerable<AllTruckResponseModel>>
            {
                Data = new List<AllTruckResponseModel> { },
                IsSuccessful = false,
                Message = "No truck found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllTruckResponseModel>>(trucks);

        return new ApiResponseModel<IEnumerable<AllTruckResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Trucks successfully retrieved",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> GetAllTrucks()
    {
       

        var trucks = await _context.Trucks.ToListAsync();

        if (!trucks.Any())
        {
            return new ApiResponseModel<IEnumerable<AllTruckResponseModel>>
            {
                Data = new List<AllTruckResponseModel> { },
                IsSuccessful = false,
                Message = "No truck found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllTruckResponseModel>>(trucks);
        foreach (var truck in data)
        {
            if (truck.DriverId != null) {
                var driver = await _context.Drivers.Where(x => x.Id == truck.DriverId).FirstOrDefaultAsync();
                truck.DriverName = driver.Name;
            }
            if (truck.TruckOwnerId != null) {
                var truckOwner = await _context.TruckOwners.Where(x => x.Id == truck.TruckOwnerId).FirstOrDefaultAsync();
                truck.TruckOwnerName = truckOwner.Name; // Check for null before accessing Name
            }
          
        }
        return new ApiResponseModel<IEnumerable<AllTruckResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Trucks successfully retrieved",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId)
    {
        var truck = await _context.Trucks.FindAsync(truckId);

        if(truck  == null)
        {
            return new ApiResponseModel<IEnumerable<string>>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "Truck not found",
            };
        }

        var documents = truck.Documents;
        return new ApiResponseModel<IEnumerable<string>>
        {
            Data = documents,
            StatusCode = 200,
            IsSuccessful = true,
            Message = "Documents retrived succesfully"
        };
    }

    public async Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
    {
        var truck = await _context.Trucks.FindAsync(model.TruckId);
        var driver = await _context.Drivers.FindAsync(model.DriverId);

        if(truck == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                StatusCode = 404,
                Message = "Truck not found"
            };
        }
        
        if (driver == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                StatusCode = 404,
                Message = "Driver not found"
            };
        }

        truck.DriverId = model.DriverId;

        _context.Trucks.Update(truck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Driver assigned to truck successfully",
            Data = true
        };
    }

    public async Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
    {
        var truck = await _context.Trucks.FindAsync(truckId);
        if(truck == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                StatusCode = 404,
                Message = "Truck not found"
            };
        }

        truck.TruckStatus = model.TruckStatus;

        _context.Trucks.Update(truck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Truck status succesfully updated",
            Data = ""
        };
    }

    public async Task<ApiResponseModel<AllOfficerResponseModel>> GetOfficerById(string officerId)
    {
        var officer = await _context.Officers.FindAsync(officerId);

        if (officer == null)
        {
            return new ApiResponseModel<AllOfficerResponseModel>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "Officer not found",
            };
        }

        var officerToReturn = _mapper.Map<AllOfficerResponseModel>(officer);

        //var documents = truck.Documents;
        return new ApiResponseModel<AllOfficerResponseModel>
        {
            Data = officerToReturn,
            StatusCode = 200,
            IsSuccessful = true,
            Message = "Officer retrived successfully"
        };
    }

    public async Task<ApiResponseModel<string>> DeleteOfficers(string officerId)
    {
        var officer = await _context.Officers.FindAsync(officerId);
        if (officer == null)
        {
            return new ApiResponseModel<string>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "Officer not found",
            };
        }

        _context.Officers.Remove(officer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            StatusCode = 200,
            IsSuccessful = true,
            Message = "Officer deleted successfully",
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllOfficerResponseModel>>> SearchOfficer(string? searchWords)
    {
        IQueryable<Officer> query = _context.Officers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.OfficerName.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var officers = await query.ToListAsync();

        if (!officers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllOfficerResponseModel>>
            {
                Data = new List<AllOfficerResponseModel> { },
                IsSuccessful = false,
                Message = "No officer found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllOfficerResponseModel>>(officers);

        return new ApiResponseModel<IEnumerable<AllOfficerResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Officers successfully retrieved",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<string>> AddNewCustomer(AddCustomerRequestModel model)
    {
        var existingCustomer = await _context.Customers.FirstOrDefaultAsync(m => m.EmailAddress == model.EmailAddress || m.PhoneNumber == model.PhoneNumber);
        if (existingCustomer != null)
        {
            if (existingCustomer.EmailAddress == model.EmailAddress)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Email address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
            else if (existingCustomer.PhoneNumber == model.PhoneNumber)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Phone Number address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
        }

        var newCustomer = new Customer
        {
            CustomerName = model.CustomerName,
            PhoneNumber = model.PhoneNumber,
            EmailAddress = model.EmailAddress,
            Location = model.Location
        };

        _context.Customers.Add(newCustomer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Customer added successfully",
            StatusCode = 201
        };
    }

    public async Task<ApiResponseModel<string>> EditCustomer(EditCustomerRequestModel model)
    {
        var customer = await _context.Customers.FindAsync(model.CustomerId);
        if(customer == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Customer not found",
                StatusCode = 404
            };
        }

        customer.CustomerName = model.CustomerName;
        customer.PhoneNumber = model.PhoneNumber;
        customer.EmailAddress = model.EmailAddress;
        customer.Location = model.Location;
        customer.RCNo = model.RCNo;
        customer.UpdatedAt = DateTime.Now.ToUniversalTime();

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Customer updated successfully",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<AllCustomerResponseModel>> GetCustomerById(string customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if(customer == null)
        {
            return new ApiResponseModel<AllCustomerResponseModel>
            {
                Data = new AllCustomerResponseModel { },
                IsSuccessful = false,
                StatusCode = 404,
                Message = "Customer not found"
            };
        }

        //var managerToReturn = _mapper.Map<AllManagerResponseModel>(manager);
        var customerToReturn = _mapper.Map<AllCustomerResponseModel>(customer);

        return new ApiResponseModel<AllCustomerResponseModel>
        {
            Data = customerToReturn,
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Customer successfully retrived"
        };
    }

    public async Task<ApiResponseModel<List<AllCustomerResponseModel>>> GetAllCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        if(customers == null)
        {
            return new ApiResponseModel<List<AllCustomerResponseModel>>
            {
                Data = new List<AllCustomerResponseModel> { },
                IsSuccessful = false,
                StatusCode = 404,
                Message = "No customers found"
            };
        }

        var customersToReturn = _mapper.Map<List<AllCustomerResponseModel>>(customers);

        return new ApiResponseModel<List<AllCustomerResponseModel>>
        {
            Data = customersToReturn,
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Customers retrived successfully"
        };
    }

    public async Task<ApiResponseModel<string>> DeleteCustomer(string customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if(customer == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Customer not found",
                StatusCode = 404
            };
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Customer deleted",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<string>> CreateNewOrder(CreateOrderRequestModel model)
    {
        string orderId = GenerateOrderId();

        //string startDate = model.StartDate;
        var fieldOfficer = await _context.Officers.Where(x => x.Id == model.FieldOfficerId).FirstOrDefaultAsync();
        if (fieldOfficer == null)
            return new ApiResponseModel<string> { IsSuccessful = false, Message = "Field officer inactive or not found", StatusCode = 404 };
        var business = await _context.Businesses.Where(x => x.Id == model.CompanyId).FirstOrDefaultAsync();
        if(business == null )
            return new ApiResponseModel<string> { IsSuccessful = false, Message = "Business not found or Inactive", StatusCode = 400 };
        
        var order = new Order
        {
            OrderId = orderId,
            CargoType = model.CargoType,
            Quantity = model.Quantity,
            OfficerId = fieldOfficer.Id,
            ManagerId = business.managerId,
            BusinessId = business.Id,
            OrderStatus = OrderStatus.Pending,
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Order created successfully",
            StatusCode = 200,
            Data = order.OrderId
        };
    }

    public async Task<ApiResponseModel<string>> AssignTruckToOrder(AssignTruckRequestModel model)
    {
        var order = await _context.Orders.Where(x => x.OrderId == model.OrderId).FirstOrDefaultAsync();
        if (order == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }
        string startDate = model.StartDate;

        order.RouteId = model.RouteId;
        order.TruckId = model.TruckId;
        //order.Price = model.Price;
        order.CustomerId = model.CustomerId;
        order.StartDate = DateTime.Parse(startDate);
        order.EndDate = DateTime.Parse(startDate).AddHours(24);
        order.DeliveryAddress = model.DeliveryAddress;

        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Order updated successfully",
            StatusCode = 200,
            Data = order.Id
        };
    }

    public async Task<ApiResponseModel<string>> EditOrder(EditOrderRequestModel model)
    {
        // Find the order by ID
        var order = await _context.Orders.FindAsync(model.Id);
        if (order == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }
        //string startDate = model.StartDate;

        //order.TruckNo = model.TruckNo;
        order.Quantity = model.Quantity;
        order.OrderStatus = model.OrderStatus;
        //order.StartDate = DateTime.Parse(startDate);
        //order.EndDate = DateTime.Parse(startDate).AddHours(24);
        order.OfficerId = model.FieldOfficerId;


        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Order updated successfully",
            StatusCode = 200,
            Data = order.OrderId
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> GetAllOrders()
    {
        try
        {
            var ordersWithDetails = await _context.Orders
                .Include(o => o.Officer)
                .Include(o => o.Business)
                .Include(o => o.Manager)
                .Include(o => o.Truck)
                .Include(o => o.Routes)
                .Include(o => o.Customer)
                .ToListAsync();

            if (ordersWithDetails == null || !ordersWithDetails.Any())
            {
                return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "No orders found",
                    StatusCode = 404,
                    Data = new List<AllOrderResponseModel> { }
                };
            }

            var orderResponseList = ordersWithDetails.Select(order => new AllOrderResponseModel
            {
                Id = order.Id, // Assuming there's a property called Id in Order entity
                OrderId = order.OrderId,
                TruckNo = order.Truck?.PlateNumber ?? "",
                Quantity = order.Quantity,
                OrderStatus = order.OrderStatus,
                FieldOfficer =  _mapper.Map<AllOfficerResponseModel>(order.Officer),
                Route =_mapper.Map<RouteResponseModel>(order.Routes),
                Business = _mapper.Map<AllBusinessResponseModel>(order.Business),
            });

            return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
            {
                IsSuccessful = true,
                Message = "Orders retrieved successfully",
                StatusCode = 200,
                Data = orderResponseList
            };
        }
        catch (Exception ex)
        {
            // Log the exception
            return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
            {
                IsSuccessful = false,
                Message = "Failed to retrieve orders",
                StatusCode = 500,
                Data = null
            };
        }
    }

    public async Task<ApiResponseModel<OrderResponseModel>> GetOrderById(string orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Officer)
            .Include(o => o.Truck)
            .Include(o => o.Routes)
            .Where(x => x.OrderId == orderId).FirstOrDefaultAsync();


        if(order == null)
        {
            return new ApiResponseModel<OrderResponseModel>
            {
                IsSuccessful = false,
                Message = "Failed to retrieve orders",
                StatusCode = 500,
                Data = new OrderResponseModel { }
            };
        }

        var orderResponseModel = new OrderResponseModel
        {
            Id = order.Id,
            OrderId = order.OrderId,
            TruckNo = order.Truck?.PlateNumber ?? "",
            Quantity = order.Quantity,
            CargoType = order.CargoType,
            OrderStatus = order.OrderStatus,
            FieldOfficerName = order.Officer.OfficerName,
            RouteFrom = order.Routes?.FromRoute ?? "",
            RouteTo = order.Routes?.ToRoute ?? "",
            StartDate = order.StartDate.ToString(),
            EndDate = order.EndDate.ToString(),
            Price = order.Routes?.Price,
            Driver = order.Truck?.DriverId ?? ""
        };

        return new ApiResponseModel<OrderResponseModel>
        {
            IsSuccessful = true,
            Message = "Order retrieved successfully",
            StatusCode = 200,
            Data = orderResponseModel
        };
    }

    private string GenerateOrderId()
    {
        Random random = new Random();
        string randomNumber = random.Next(1000, 9999).ToString(); // Generate a random 4-digit number
        return "TR" + randomNumber;
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

    public async Task<ApiResponseModel<DashboardSummaryResponse>> GetDashBoardData()
    {
        int totalBusiness = await _context.Businesses.CountAsync();
        int totalCustomers = await _context.Customers.CountAsync();

        int totalTrucks = await _context.Trucks.CountAsync();

        int totalActiveTrucks = await _context.Trucks.CountAsync(t => t.TruckStatus == TruckStatus.EnRoute || t.TruckStatus == TruckStatus.Available);

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
    public async Task<ApiResponseModel<IEnumerable<TruckOwnerResponseModel>>> SearchTruckOwners(string searchWords)
    {
        IQueryable<TruckOwner> query = _context.TruckOwners;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.Name.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var owners = await query.ToListAsync();

        if (!owners.Any())
        {
            return new ApiResponseModel<IEnumerable<TruckOwnerResponseModel>>
            {
                Data = new List<TruckOwnerResponseModel> { },
                IsSuccessful = false,
                Message = "No Truck Owner found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<TruckOwnerResponseModel>>(owners);

        return new ApiResponseModel<IEnumerable<TruckOwnerResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Truck Owner successfully retrieved",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllCustomerResponseModel>>> SearchCustomers(string searchWords)
    {
        IQueryable<Customer> query = _context.Customers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " && searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.CustomerName.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var customers = await query.ToListAsync();

        if (!customers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllCustomerResponseModel>>
            {
                Data = new List<AllCustomerResponseModel> { },
                IsSuccessful = false,
                Message = "No Customer found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllCustomerResponseModel>>(customers);

        return new ApiResponseModel<IEnumerable<AllCustomerResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Customers successfully retrieved",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<GtvDashboardSummary>> GetGtvDashBoardSummary(DateTime startDate, DateTime endDate)
    {
        // validate satrtDate and endDate
        if(startDate > endDate)
        {
            return new ApiResponseModel<GtvDashboardSummary>
            {
                Data = null,
                IsSuccessful = false,
                Message = "Start date must be less than or equal to end datae",
                StatusCode = 400
            };
        }

        List<Order> orders = await _context.Orders.Where(o => o.StartDate >= startDate && o.EndDate <= endDate).ToListAsync();

        decimal totalGtv = orders.Sum(o => decimal.Parse(o.Routes?.Gtv ?? "0"));
        decimal totalRevenue = orders.Sum(o => decimal.Parse(o.Routes?.Price.ToString() ?? "0"));

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

        return new ApiResponseModel<TruckDahsBoardData> { Data = stats, IsSuccessful = true, Message = "Dashboard data", StatusCode = 200 };
    }

    public async Task<ApiResponseModel<ManagerDashboardData>> GetManagerDashboardData(string managerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Manager)
            .Where(o => o.ManagerId == managerId)
            .ToListAsync();

        int completedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
        int flaggedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Flagged);
        decimal totalOrderPrice = orders.Sum(o => decimal.Parse(o.Routes?.Price.ToString() ?? "0"));

        var stats = new ManagerDashboardData
        {
            CompletedOrders = completedOrders,
            FlaggedOrders = flaggedOrders,
            TotalOrderPrice = totalOrderPrice
        };

        return new ApiResponseModel<ManagerDashboardData>
        {
            Data = stats,
            IsSuccessful = true,
            Message = "Dashboard data",
            StatusCode = 200
        };
    }
}