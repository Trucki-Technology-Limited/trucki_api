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

public class TruckRepository:ITruckRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public TruckRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
    }
    
    
    public async Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model)
    {
        var existingTruck = await _context.Trucks.Where(x => x.PlateNumber == model.PlateNumber).FirstOrDefaultAsync();
        if (existingTruck != null)
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
        if (truck == null)
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
        if (truck == null)
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
        if (truck == null)
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

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " &&
            searchWords.ToLower() != "null")
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
            if (truck.DriverId != null)
            {
                var driver = await _context.Drivers.Where(x => x.Id == truck.DriverId).FirstOrDefaultAsync();
                truck.DriverName = driver.Name;
            }

            if (truck.TruckOwnerId != null)
            {
                var truckOwner =
                    await _context.TruckOwners.Where(x => x.Id == truck.TruckOwnerId).FirstOrDefaultAsync();
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

        if (truck == null)
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

        if (truck == null)
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

        truck.Driver = driver;

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
        if (truck == null)
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

    
}