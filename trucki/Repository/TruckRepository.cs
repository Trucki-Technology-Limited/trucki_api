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

public class TruckRepository : ITruckRepository
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
            TruckStatus = TruckStatus.Available,
            TruckOwner = truckOwner,
            TruckType = model.TruckType,
            TruckName = model.TruckName,
            TruckLicenseExpiryDate = model.TruckLicenseExpiryDate,
            RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate,
            InsuranceExpiryDate = model.InsuranceExpiryDate
        };



        newTruck.Documents = model.Documents;

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
        //truck.CertOfOwnerShip = model.CertOfOwnerShip;
        truck.PlateNumber = model.PlateNumber;
        truck.TruckCapacity = model.TruckCapacity;
        truck.DriverId = model.DriverId;
        //truck.Capacity = model.Capacity;
        truck.TruckName = model.TruckName;
        truck.TruckType = model.TruckType;
        truck.TruckLicenseExpiryDate = model.TruckLicenseExpiryDate;
        truck.RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate;
        truck.InsuranceExpiryDate = model.InsuranceExpiryDate;

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
        var truck = await _context.Trucks.Where(x => x.Id == truckId).Include(e => e.Driver).FirstOrDefaultAsync();
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

        var truckOwner = truck.TruckOwnerId != null ?
            await _context.TruckOwners.Where(x => x.Id == truck.TruckOwnerId).FirstOrDefaultAsync() : null;

        var truckToReturn = new AllTruckResponseModel
        {
            Id = truck.Id,
            Documents = truck.Documents,
            PlateNumber = truck.PlateNumber,
            TruckCapacity = truck.TruckCapacity,
            DriverId = truck.DriverId,
            DriverName = driver?.Name,
            TruckName = truck.TruckName,
            TruckStatus = truck.TruckStatus,
            Capacity = truck.TruckCapacity,
            TruckOwnerId = truck.TruckOwnerId,
            TruckOwnerName = truckOwner?.Name,
            TruckType = truck.TruckType,
            TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
            RoadWorthinessExpiryDate = truck.RoadWorthinessExpiryDate,
            InsuranceExpiryDate = truck.InsuranceExpiryDate,
            ApprovalStatus = truck.ApprovalStatus,
            IsDriverOwnedTruck = truck.IsDriverOwnedTruck,
            ExternalTruckPictureUrl = truck.ExternalTruckPictureUrl,
            CargoSpacePictureUrl = truck.CargoSpacePictureUrl,
            CreatedAt = truck.CreatedAt
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

    public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetAllTrucks()
    {
        var trucks = await _context.Trucks
           .Include(t => t.Driver)
           .Include(t => t.TruckOwner)
           .ToListAsync();

        if (!trucks.Any())
        {
            return new ApiResponseModel<List<AllTruckResponseModel>>
            {
                Data = new List<AllTruckResponseModel> { },
                IsSuccessful = false,
                Message = "No truck found",
                StatusCode = 404
            };
        }

        var data = new List<AllTruckResponseModel>();

        foreach (var truck in trucks)
        {
            // Manually map each Truck to AllTruckResponseModel
            var responseModel = new AllTruckResponseModel
            {
                Id = truck.Id,
                Documents = truck.Documents ?? new List<string>(), // Ensure Documents is not null
                PlateNumber = truck.PlateNumber,
                TruckCapacity = truck.TruckCapacity,
                DriverId = truck.DriverId,
                DriverName = truck.Driver?.Name, // Handle null Driver
                Capacity = truck.TruckCapacity,
                TruckOwnerId = truck.TruckOwnerId,
                TruckOwnerName = truck.TruckOwner?.Name, // Handle null TruckOwner
                TruckName = truck.TruckName,
                TruckType = truck.TruckType,
                TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
                RoadWorthinessExpiryDate = truck.RoadWorthinessExpiryDate,
                InsuranceExpiryDate = truck.InsuranceExpiryDate,
                TruckNumber = truck.TruckiNumber,
                TruckStatus = truck.TruckStatus,
                ApprovalStatus = truck.ApprovalStatus,
                IsDriverOwnedTruck = truck.IsDriverOwnedTruck,
                ExternalTruckPictureUrl = truck.ExternalTruckPictureUrl,
                CargoSpacePictureUrl = truck.CargoSpacePictureUrl,
                CreatedAt = truck.CreatedAt
            };

            data.Add(responseModel);
        }

        return new ApiResponseModel<List<AllTruckResponseModel>>
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

        // 1. If the truck is already assigned to a different driver, unassign that driver first
        var existingDriverForThisTruck = await _context.Drivers
            .FirstOrDefaultAsync(d => d.TruckId == truck.Id);

        if (existingDriverForThisTruck != null && existingDriverForThisTruck.Id != driver.Id)
        {
            existingDriverForThisTruck.TruckId = null;
            existingDriverForThisTruck.Truck = null;
            _context.Drivers.Update(existingDriverForThisTruck);
        }

        // 2. If the new driver is already assigned to a different truck, unassign them from that old truck
        //    so we don’t have two trucks referencing the same driver.
        var oldTruckForThisDriver = await _context.Trucks
            .FirstOrDefaultAsync(t => t.DriverId == driver.Id);

        if (oldTruckForThisDriver != null && oldTruckForThisDriver.Id != truck.Id)
        {
            oldTruckForThisDriver.DriverId = null;
            oldTruckForThisDriver.Driver = null;
            _context.Trucks.Update(oldTruckForThisDriver);
        }

        // 3. Assign the new driver to the current truck
        truck.Driver = driver;
        truck.DriverId = driver.Id;
        driver.Truck = truck;
        driver.TruckId = truck.Id;

        _context.Trucks.Update(truck);
        _context.Drivers.Update(driver);

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

    public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByOwnersId(string ownersId)
    {
        // Retrieve all trucks for the specified owner
        var trucks = await _context.Trucks.Where(x => x.TruckOwnerId == ownersId).ToListAsync();

        // Check if any trucks were found
        if (trucks == null || !trucks.Any())
        {
            return new ApiResponseModel<List<AllTruckResponseModel>>
            {
                IsSuccessful = false,
                Message = "No trucks found for this owner",
                StatusCode = 404,
                Data = new List<AllTruckResponseModel>()
            };
        }

        // Prepare the response model list
        var truckResponses = new List<AllTruckResponseModel>();

        foreach (var truck in trucks)
        {
            var driver = await _context.Drivers.Where(x => x.Id == truck.DriverId).FirstOrDefaultAsync();
            var truckOwner = await _context.TruckOwners.Where(x => x.Id == truck.TruckOwnerId).FirstOrDefaultAsync();

            var truckToReturn = new AllTruckResponseModel
            {
                Id = truck.Id,
                Documents = truck.Documents,
                PlateNumber = truck.PlateNumber,
                TruckCapacity = truck.TruckCapacity,
                DriverId = truck.DriverId,
                TruckName = truck.TruckName,
                TruckStatus = truck.TruckStatus,
                DriverName = driver?.Name, // Safe navigation in case driver is null
                TruckOwnerId = truck.TruckOwnerId,
                TruckOwnerName = truckOwner?.Name, // Safe navigation in case truckOwner is null
                TruckType = truck.TruckType,
                TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
                RoadWorthinessExpiryDate = truck.RoadWorthinessExpiryDate,
                InsuranceExpiryDate = truck.InsuranceExpiryDate,
                CreatedAt = truck.CreatedAt
            };

            truckResponses.Add(truckToReturn);
        }

        return new ApiResponseModel<List<AllTruckResponseModel>>
        {
            IsSuccessful = true,
            Message = "Trucks found",
            StatusCode = 200,
            Data = truckResponses
        };
    }
    public async Task<ApiResponseModel<TruckStatusCountResponseModel>> GetTruckStatusCountByOwnerId(string ownerId)
    {
        // Group trucks by status for the specified owner
        var trucksGroupedByStatus = await _context.Trucks
            .Where(x => x.TruckOwnerId == ownerId)
            .GroupBy(x => x.TruckStatus)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        // Prepare the response with default values for statuses
        var truckStatusCount = new TruckStatusCountResponseModel
        {
            EnRouteCount = trucksGroupedByStatus.FirstOrDefault(x => x.Status == TruckStatus.EnRoute)?.Count ?? 0,
            AvailableCount = trucksGroupedByStatus.FirstOrDefault(x => x.Status == TruckStatus.Available)?.Count ?? 0,
            OutOfServiceCount = trucksGroupedByStatus.FirstOrDefault(x => x.Status == TruckStatus.OutOfService)?.Count ?? 0
        };

        // Return successful response with truck status counts
        return new ApiResponseModel<TruckStatusCountResponseModel>
        {
            IsSuccessful = true,
            Message = "Truck status counts found",
            StatusCode = 200,
            Data = truckStatusCount
        };
    }
    public async Task<ApiResponseModel<string>> UpdateApprovalStatusAsync(string truckId, ApprovalStatus approvalStatus)
    {
        var truck = await _context.Trucks.FindAsync(truckId);
        if (truck == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Truck not found",
                StatusCode = 404
            };
        }

        truck.ApprovalStatus = approvalStatus;
        _context.Trucks.Update(truck);
        await _context.SaveChangesAsync();

        string statusMessage = approvalStatus switch
        {
            ApprovalStatus.Approved => "Truck approved successfully",
            ApprovalStatus.NotApproved => "Truck marked as not approved",
            ApprovalStatus.Blocked => "Truck blocked successfully",
            _ => "Status updated successfully"
        };

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = statusMessage,
            StatusCode = 200,
            Data = truck.Id
        };
    }

    public async Task<ApiResponseModel<string>> AddDriverOwnedTruck(DriverAddTruckRequestModel model)
    {
        var existingTruck = await _context.Trucks.Where(x => x.PlateNumber == model.PlateNumber).FirstOrDefaultAsync();
        if (existingTruck != null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Truck with this plate number already exists",
                StatusCode = 400
            };
        }

        var driver = await _context.Drivers.FindAsync(model.DriverId);
        if (driver == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Driver not found",
                StatusCode = 404
            };
        }

        // Check if the driver already has a truck
        var existingDriverTruck = await _context.Trucks.Where(x => x.DriverId == model.DriverId).FirstOrDefaultAsync();
        if (existingDriverTruck != null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "You already have a truck registered. Please contact admin to update your truck details if needed.",
                StatusCode = 400
            };
        }

        var newTruck = new Truck
        {
            PlateNumber = model.PlateNumber,
            TruckCapacity = model.TruckCapacity,
            TruckName = model.TruckName,
            TruckType = model.TruckType,
            TruckLicenseExpiryDate = model.TruckLicenseExpiryDate,
            RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate,
            InsuranceExpiryDate = model.InsuranceExpiryDate,
            Documents = model.Documents,
            DriverId = model.DriverId,
            Driver = driver,
            TruckStatus = TruckStatus.OutOfService, // Set as out of service until approved
            ApprovalStatus = ApprovalStatus.Pending, // Set as pending approval
            IsDriverOwnedTruck = true, // Indicate this truck is driver-owned

            // New picture fields
            ExternalTruckPictureUrl = model.ExternalTruckPictureUrl,
            CargoSpacePictureUrl = model.CargoSpacePictureUrl,
        };

        _context.Trucks.Add(newTruck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Your truck has been submitted for review. You will be notified once it's approved.",
            StatusCode = 201,
            Data = newTruck.Id
        };
    }
    public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByDriverId(string driverId)
    {
        // Retrieve the truck for the specified driver (since a driver can only have one truck)
        var truckRes = await _context.Trucks
            .Where(x => x.DriverId == driverId)
            .FirstOrDefaultAsync();

        // Check if any truck was found
        if (truckRes == null)
        {
            return new ApiResponseModel<List<AllTruckResponseModel>>
            {
                IsSuccessful = false,
                Message = "No truck found for this driver",
                StatusCode = 404,
                Data = new List<AllTruckResponseModel>()
            };
        }

        var trucks = new List<Truck> { truckRes };

        // Prepare the response model list
        var truckResponses = new List<AllTruckResponseModel>();

        foreach (var truck in trucks)
        {
            var driver = await _context.Drivers.Where(x => x.Id == truck.DriverId).FirstOrDefaultAsync();
            var truckOwner = truck.TruckOwnerId != null ?
                await _context.TruckOwners.Where(x => x.Id == truck.TruckOwnerId).FirstOrDefaultAsync() : null;

            var truckToReturn = new AllTruckResponseModel
            {
                Id = truck.Id,
                Documents = truck.Documents,
                PlateNumber = truck.PlateNumber,
                TruckCapacity = truck.TruckCapacity,
                DriverId = truck.DriverId,
                TruckName = truck.TruckName,
                TruckStatus = truck.TruckStatus,
                DriverName = driver?.Name, // Safe navigation in case driver is null
                TruckOwnerId = truck.TruckOwnerId,
                TruckOwnerName = truckOwner?.Name, // Safe navigation in case truckOwner is null
                TruckType = truck.TruckType,
                TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
                RoadWorthinessExpiryDate = truck.RoadWorthinessExpiryDate,
                InsuranceExpiryDate = truck.InsuranceExpiryDate,
                IsDriverOwnedTruck = truck.IsDriverOwnedTruck,
                ApprovalStatus = truck.ApprovalStatus,
                ExternalTruckPictureUrl = truck.ExternalTruckPictureUrl,
                CargoSpacePictureUrl = truck.CargoSpacePictureUrl,
                CreatedAt = truck.CreatedAt
            };

            truckResponses.Add(truckToReturn);
        }

        return new ApiResponseModel<List<AllTruckResponseModel>>
        {
            IsSuccessful = true,
            Message = "Trucks found",
            StatusCode = 200,
            Data = truckResponses
        };
    }

    public async Task<ApiResponseModel<bool>> UpdateTruckPhotos(UpdateTruckPhotosRequestModel model)
    {
        var truck = await _context.Trucks.FindAsync(model.TruckId);
        if (truck == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Truck not found",
                StatusCode = 404,
                Data = false
            };
        }

        // Update truck photo fields
        if (!string.IsNullOrEmpty(model.ExternalTruckPictureUrl))
        {
            truck.ExternalTruckPictureUrl = model.ExternalTruckPictureUrl;
        }

        if (!string.IsNullOrEmpty(model.CargoSpacePictureUrl))
        {
            truck.CargoSpacePictureUrl = model.CargoSpacePictureUrl;
        }

        _context.Trucks.Update(truck);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Truck photos updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

}