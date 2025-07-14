using System.Text;
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
    /// <summary>
    /// Get all trucks with enhanced filtering, pagination, and search
    /// </summary>
    public async Task<ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>> GetAllTrucksEnhancedAsync(GetTrucksQueryDto query)
    {
        try
        {
            query.ValidateAndNormalize();

            // Build the base query with necessary includes
            IQueryable<Truck> trucksQuery = _context.Trucks
                .Include(t => t.Driver)
                .Include(t => t.TruckOwner);

            // Apply search filter
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                trucksQuery = trucksQuery.Where(t =>
                    t.PlateNumber.ToLower().Contains(searchTerm) ||
                    (t.TruckName != null && t.TruckName.ToLower().Contains(searchTerm)));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(query.TruckType))
            {
                trucksQuery = trucksQuery.Where(t => t.TruckType == query.TruckType);
            }

            if (query.TruckStatus.HasValue)
            {
                trucksQuery = trucksQuery.Where(t => t.TruckStatus == query.TruckStatus.Value);
            }

            if (query.ApprovalStatus.HasValue)
            {
                trucksQuery = trucksQuery.Where(t => t.ApprovalStatus == query.ApprovalStatus.Value);
            }

            if (query.IsDriverOwned.HasValue)
            {
                trucksQuery = trucksQuery.Where(t => t.IsDriverOwnedTruck == query.IsDriverOwned.Value);
            }

            if (!string.IsNullOrEmpty(query.TruckOwnerId))
            {
                trucksQuery = trucksQuery.Where(t => t.TruckOwnerId == query.TruckOwnerId);
            }

            // Apply sorting
            trucksQuery = query.SortBy.ToLower() switch
            {
                "platenumber" => query.SortDescending
                    ? trucksQuery.OrderByDescending(t => t.PlateNumber)
                    : trucksQuery.OrderBy(t => t.PlateNumber),
                "truckname" => query.SortDescending
                    ? trucksQuery.OrderByDescending(t => t.TruckName)
                    : trucksQuery.OrderBy(t => t.TruckName),
                "trucktype" => query.SortDescending
                    ? trucksQuery.OrderByDescending(t => t.TruckType)
                    : trucksQuery.OrderBy(t => t.TruckType),
                "truckstatus" => query.SortDescending
                    ? trucksQuery.OrderByDescending(t => t.TruckStatus)
                    : trucksQuery.OrderBy(t => t.TruckStatus),
                "approvalstatus" => query.SortDescending
                    ? trucksQuery.OrderByDescending(t => t.ApprovalStatus)
                    : trucksQuery.OrderBy(t => t.ApprovalStatus),
                _ => query.SortDescending
                    ? trucksQuery.OrderByDescending(t => t.CreatedAt)
                    : trucksQuery.OrderBy(t => t.CreatedAt)
            };

            // Get total count before pagination
            var totalCount = await trucksQuery.CountAsync();

            // Apply pagination
            var trucks = await trucksQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            // Build response models with order statistics
            var enhancedTrucks = new List<EnhancedTruckResponseModel>();

            foreach (var truck in trucks)
            {
                var enhancedTruck = await BuildEnhancedTruckResponseModel(truck);
                enhancedTrucks.Add(enhancedTruck);
            }

            var pagedResponse = new PagedResponse<EnhancedTruckResponseModel>
            {
                Data = enhancedTrucks,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };

            return new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
            {
                IsSuccessful = true,
                Message = $"Successfully retrieved {totalCount} trucks",
                StatusCode = 200,
                Data = pagedResponse
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
            {
                IsSuccessful = false,
                Message = $"Error retrieving trucks: {ex.Message}",
                StatusCode = 500,
                Data = null
            };
        }
    }

    /// <summary>
    /// Get comprehensive truck status and approval status counts
    /// </summary>
    public async Task<ApiResponseModel<EnhancedTruckStatusCountResponseModel>> GetTruckStatusCountsEnhancedAsync()
    {
        try
        {
            var trucks = await _context.Trucks.ToListAsync();

            var statusCounts = new EnhancedTruckStatusCountResponseModel
            {
                // Truck Status Counts
                EnRouteCount = trucks.Count(t => t.TruckStatus == TruckStatus.EnRoute),
                AvailableCount = trucks.Count(t => t.TruckStatus == TruckStatus.Available),
                BusyCount = trucks.Count(t => t.TruckStatus == TruckStatus.Busy),
                OutOfServiceCount = trucks.Count(t => t.TruckStatus == TruckStatus.OutOfService),

                // Approval Status Counts
                PendingCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.Pending),
                ApprovedCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.Approved),
                NotApprovedCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.NotApproved),
                BlockedCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.Blocked),

                // Ownership Counts
                DriverOwnedTrucks = trucks.Count(t => t.IsDriverOwnedTruck),
                TruckOwnerOwnedTrucks = trucks.Count(t => !t.IsDriverOwnedTruck)
            };

            statusCounts.CalculateDerivedProperties();

            return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
            {
                IsSuccessful = true,
                Message = "Truck status counts retrieved successfully",
                StatusCode = 200,
                Data = statusCounts
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
            {
                IsSuccessful = false,
                Message = $"Error retrieving truck counts: {ex.Message}",
                StatusCode = 500,
                Data = null
            };
        }
    }

    /// <summary>
    /// Get detailed truck information by ID with driver details and trip statistics
    /// </summary>
    public async Task<ApiResponseModel<TruckDetailResponseModel>> GetTruckByIdEnhancedAsync(string truckId)
    {
        try
        {
            var truck = await _context.Trucks
                .Include(t => t.Driver)
                .Include(t => t.TruckOwner)
                .FirstOrDefaultAsync(t => t.Id == truckId);

            if (truck == null)
            {
                return new ApiResponseModel<TruckDetailResponseModel>
                {
                    IsSuccessful = false,
                    Message = "Truck not found",
                    StatusCode = 404,
                    Data = null
                };
            }

            var truckDetail = await BuildTruckDetailResponseModel(truck);

            return new ApiResponseModel<TruckDetailResponseModel>
            {
                IsSuccessful = true,
                Message = "Truck details retrieved successfully",
                StatusCode = 200,
                Data = truckDetail
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<TruckDetailResponseModel>
            {
                IsSuccessful = false,
                Message = $"Error retrieving truck details: {ex.Message}",
                StatusCode = 500,
                Data = null
            };
        }
    }

    /// <summary>
    /// Export trucks as CSV based on query filters
    /// </summary>
    public async Task<ApiResponseModel<byte[]>> ExportTrucksAsCsvAsync(GetTrucksQueryDto query)
    {
        try
        {
            // Remove pagination for export (get all matching records)
            var exportQuery = new GetTrucksQueryDto
            {
                PageNumber = 1,
                PageSize = int.MaxValue,
                SearchTerm = query.SearchTerm,
                TruckType = query.TruckType,
                TruckStatus = query.TruckStatus,
                ApprovalStatus = query.ApprovalStatus,
                IsDriverOwned = query.IsDriverOwned,
                TruckOwnerId = query.TruckOwnerId,
                SortBy = query.SortBy,
                SortDescending = query.SortDescending
            };

            var trucksResponse = await GetAllTrucksEnhancedAsync(exportQuery);

            if (!trucksResponse.IsSuccessful || trucksResponse.Data?.Data == null)
            {
                return new ApiResponseModel<byte[]>
                {
                    IsSuccessful = false,
                    Message = "No data available for export",
                    StatusCode = 404,
                    Data = null
                };
            }

            var csvData = GenerateCsvData(trucksResponse.Data.Data);

            return new ApiResponseModel<byte[]>
            {
                IsSuccessful = true,
                Message = $"Successfully exported {trucksResponse.Data.Data.Count()} trucks",
                StatusCode = 200,
                Data = csvData
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<byte[]>
            {
                IsSuccessful = false,
                Message = $"Error exporting trucks: {ex.Message}",
                StatusCode = 500,
                Data = null
            };
        }
    }

    /// <summary>
    /// Update truck approval status with enhanced functionality
    /// </summary>
    public async Task<ApiResponseModel<string>> UpdateApprovalStatusEnhancedAsync(string truckId, UpdateApprovalStatusRequest request)
    {
        try
        {
            var truck = await _context.Trucks.FindAsync(truckId);
            if (truck == null)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Truck not found",
                    StatusCode = 404,
                    Data = null
                };
            }

            // Validate rejection reason for negative statuses
            if ((request.ApprovalStatus == ApprovalStatus.NotApproved ||
                 request.ApprovalStatus == ApprovalStatus.Blocked) &&
                string.IsNullOrEmpty(request.RejectionReason))
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Rejection reason is required when marking truck as Not Approved or Blocked",
                    StatusCode = 400,
                    Data = null
                };
            }

            truck.ApprovalStatus = request.ApprovalStatus;

            // If approved, set truck status to Available (unless it's already assigned)
            if (request.ApprovalStatus == ApprovalStatus.Approved && truck.TruckStatus == TruckStatus.OutOfService)
            {
                truck.TruckStatus = TruckStatus.Available;
            }

            _context.Trucks.Update(truck);
            await _context.SaveChangesAsync();

            string statusMessage = request.ApprovalStatus switch
            {
                ApprovalStatus.Approved => "Truck approved successfully and set to Available status",
                ApprovalStatus.NotApproved => $"Truck marked as not approved. Reason: {request.RejectionReason}",
                ApprovalStatus.Blocked => $"Truck blocked successfully. Reason: {request.RejectionReason}",
                ApprovalStatus.Pending => "Truck status reset to pending",
                _ => "Approval status updated successfully"
            };

            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = statusMessage,
                StatusCode = 200,
                Data = truck.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = $"Error updating approval status: {ex.Message}",
                StatusCode = 500,
                Data = null
            };
        }
    }

    /// <summary>
    /// Get truck status counts by owner with enhanced details
    /// </summary>
    public async Task<ApiResponseModel<EnhancedTruckStatusCountResponseModel>> GetTruckStatusCountsByOwnerIdAsync(string ownerId)
    {
        try
        {
            var trucks = await _context.Trucks
                .Where(t => t.TruckOwnerId == ownerId)
                .ToListAsync();

            if (!trucks.Any())
            {
                return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                {
                    IsSuccessful = false,
                    Message = "No trucks found for this owner",
                    StatusCode = 404,
                    Data = new EnhancedTruckStatusCountResponseModel()
                };
            }

            var statusCounts = new EnhancedTruckStatusCountResponseModel
            {
                // Truck Status Counts
                EnRouteCount = trucks.Count(t => t.TruckStatus == TruckStatus.EnRoute),
                AvailableCount = trucks.Count(t => t.TruckStatus == TruckStatus.Available),
                BusyCount = trucks.Count(t => t.TruckStatus == TruckStatus.Busy),
                OutOfServiceCount = trucks.Count(t => t.TruckStatus == TruckStatus.OutOfService),

                // Approval Status Counts
                PendingCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.Pending),
                ApprovedCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.Approved),
                NotApprovedCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.NotApproved),
                BlockedCount = trucks.Count(t => t.ApprovalStatus == ApprovalStatus.Blocked),

                // All trucks for this owner are truck owner owned
                DriverOwnedTrucks = 0,
                TruckOwnerOwnedTrucks = trucks.Count
            };

            statusCounts.CalculateDerivedProperties();

            return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
            {
                IsSuccessful = true,
                Message = "Owner truck status counts retrieved successfully",
                StatusCode = 200,
                Data = statusCounts
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
            {
                IsSuccessful = false,
                Message = $"Error retrieving owner truck counts: {ex.Message}",
                StatusCode = 500,
                Data = null
            };
        }
    }
    /// <summary>
    /// Build enhanced truck response model with order statistics
    /// </summary>
    private async Task<EnhancedTruckResponseModel> BuildEnhancedTruckResponseModel(Truck truck)
    {
        // Get order statistics
        var orderStats = await GetTruckOrderStatistics(truck.Id);
        var currentTrip = await GetCurrentTripInfo(truck.Id);

        return new EnhancedTruckResponseModel
        {
            Id = truck.Id,
            PlateNumber = truck.PlateNumber,
            TruckName = truck.TruckName,
            TruckType = truck.TruckType,
            TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
            InsuranceExpiryDate = truck.InsuranceExpiryDate,
            TruckStatus = truck.TruckStatus,
            ApprovalStatus = truck.ApprovalStatus,
            TruckCapacity = truck.TruckCapacity,
            IsDriverOwnedTruck = truck.IsDriverOwnedTruck,
            Documents = truck.Documents,
            ExternalTruckPictureUrl = truck.ExternalTruckPictureUrl,
            CargoSpacePictureUrl = truck.CargoSpacePictureUrl,
            CreatedAt = truck.CreatedAt,

            // Driver details
            DriverId = truck.DriverId,
            DriverName = truck.Driver?.Name,
            DriverPhone = truck.Driver?.Phone,
            DriverEmail = truck.Driver?.EmailAddress,

            // Truck owner details
            TruckOwnerId = truck.TruckOwnerId,
            TruckOwnerName = truck.TruckOwner?.Name,

            // Order statistics
            TotalCargoOrders = orderStats.TotalCargoOrders,
            TotalNormalOrders = orderStats.TotalNormalOrders,
            IsCurrentlyOnTrip = currentTrip != null,
            CurrentTripOrderId = currentTrip?.OrderId,
            CurrentTripType = currentTrip?.OrderType
        };
    }

    /// <summary>
    /// Build detailed truck response model
    /// </summary>
    private async Task<TruckDetailResponseModel> BuildTruckDetailResponseModel(Truck truck)
    {
        var orderStats = await GetDetailedTruckOrderStatistics(truck.Id);
        var currentTrip = await GetDetailedCurrentTripInfo(truck.Id);

        var truckDetail = new TruckDetailResponseModel
        {
            Id = truck.Id,
            PlateNumber = truck.PlateNumber,
            TruckName = truck.TruckName,
            TruckType = truck.TruckType,
            TruckCapacity = truck.TruckCapacity,
            TruckLicenseExpiryDate = truck.TruckLicenseExpiryDate,
            RoadWorthinessExpiryDate = truck.RoadWorthinessExpiryDate,
            InsuranceExpiryDate = truck.InsuranceExpiryDate,
            TruckStatus = truck.TruckStatus,
            ApprovalStatus = truck.ApprovalStatus,
            IsDriverOwnedTruck = truck.IsDriverOwnedTruck,
            Documents = truck.Documents,
            ExternalTruckPictureUrl = truck.ExternalTruckPictureUrl,
            CargoSpacePictureUrl = truck.CargoSpacePictureUrl,
            TruckiNumber = truck.TruckiNumber,
            CreatedAt = truck.CreatedAt,
            OrderStatistics = orderStats,
            CurrentTrip = currentTrip
        };

        // Add driver details if available
        if (truck.Driver != null)
        {
            truckDetail.DriverDetails = new DriverDetailInfo
            {
                Id = truck.Driver.Id,
                Name = truck.Driver.Name,
                Phone = truck.Driver.Phone,
                EmailAddress = truck.Driver.EmailAddress,
                Country = truck.Driver.Country,
                IsActive = truck.Driver.IsActive,
                DriversLicence = truck.Driver.DriversLicence,
                PassportFile = truck.Driver.PassportFile,
                OnboardingStatus = truck.Driver.OnboardingStatus
            };
        }

        // Add truck owner details if available
        if (truck.TruckOwner != null)
        {
            truckDetail.TruckOwnerDetails = new TruckOwnerDetailInfo
            {
                Id = truck.TruckOwner.Id,
                Name = truck.TruckOwner.Name,
                Phone = truck.TruckOwner.Phone,
                EmailAddress = truck.TruckOwner.EmailAddress,
                Address = truck.TruckOwner.Address
            };
        }

        return truckDetail;
    }

    /// <summary>
    /// Get basic order statistics for truck
    /// </summary>
    private async Task<(int TotalCargoOrders, int TotalNormalOrders)> GetTruckOrderStatistics(string truckId)
    {
        // Get cargo orders count where this truck was selected
        var cargoOrdersCount = await _context.Set<CargoOrders>()
            .Where(co => co.AcceptedBid != null &&
                       co.AcceptedBid.TruckId == truckId)
            .CountAsync();

        // Get normal orders count
        var normalOrdersCount = await _context.Orders
            .Where(o => o.TruckId == truckId)
            .CountAsync();

        return (cargoOrdersCount, normalOrdersCount);
    }

    /// <summary>
    /// Get detailed order statistics for truck detail view
    /// </summary>
    private async Task<TruckOrderStatistics> GetDetailedTruckOrderStatistics(string truckId)
    {
        // Get cargo orders statistics
        var cargoOrders = await _context.Set<CargoOrders>()
            .Where(co => co.AcceptedBid != null && co.AcceptedBid.TruckId == truckId)
            .ToListAsync();

        var completedCargoOrders = cargoOrders.Count(co => co.Status == CargoOrderStatus.Completed);

        // Get normal orders statistics
        var normalOrders = await _context.Orders
            .Where(o => o.TruckId == truckId)
            .ToListAsync();

        var completedNormalOrders = normalOrders.Count(o => o.OrderStatus == OrderStatus.Delivered);

        // Calculate total earnings (you may need to adjust this based on your business logic)
        var totalEarnings = cargoOrders.Where(co => co.Status == CargoOrderStatus.Completed)
                                     .Sum(co => co.DriverEarnings ?? 0);

        // Get last trip date
        var lastCargoOrderDate = cargoOrders.Any() ? cargoOrders.Max(co => co.CreatedAt) : (DateTime?)null;
        var lastNormalOrderDate = normalOrders.Any() ? normalOrders.Max(o => o.CreatedAt) : (DateTime?)null;

        DateTime? lastTripDate = null;
        if (lastCargoOrderDate.HasValue && lastNormalOrderDate.HasValue)
        {
            lastTripDate = lastCargoOrderDate > lastNormalOrderDate ? lastCargoOrderDate : lastNormalOrderDate;
        }
        else if (lastCargoOrderDate.HasValue)
        {
            lastTripDate = lastCargoOrderDate;
        }
        else if (lastNormalOrderDate.HasValue)
        {
            lastTripDate = lastNormalOrderDate;
        }

        // Check if currently on trip
        var isOnTrip = await IsCurrentlyOnTrip(truckId);

        return new TruckOrderStatistics
        {
            TotalCargoOrders = cargoOrders.Count,
            CompletedCargoOrders = completedCargoOrders,
            TotalNormalOrders = normalOrders.Count,
            CompletedNormalOrders = completedNormalOrders,
            IsCurrentlyOnTrip = isOnTrip,
            TotalEarnings = totalEarnings,
            LastTripDate = lastTripDate
        };
    }

    /// <summary>
    /// Get current trip information
    /// </summary>
    private async Task<(string OrderId, string OrderType)?> GetCurrentTripInfo(string truckId)
    {
        // Check for active cargo orders
        var activeCargoOrder = await _context.Set<CargoOrders>()
            .Where(co => co.AcceptedBid != null &&
                       co.AcceptedBid.TruckId == truckId &&
                       (co.Status == CargoOrderStatus.InTransit ||
                        co.Status == CargoOrderStatus.ReadyForPickup ||
                        co.Status == CargoOrderStatus.DriverAcknowledged))
            .FirstOrDefaultAsync();

        if (activeCargoOrder != null)
        {
            return (activeCargoOrder.Id, "Cargo");
        }

        // Check for active normal orders
        var activeNormalOrder = await _context.Orders
            .Where(o => o.TruckId == truckId &&
                       (o.OrderStatus == OrderStatus.InTransit ||
                        o.OrderStatus == OrderStatus.Loaded ||
                        o.OrderStatus == OrderStatus.Assigned))
            .FirstOrDefaultAsync();

        if (activeNormalOrder != null)
        {
            return (activeNormalOrder.Id, "Normal");
        }

        return null;
    }

    /// <summary>
    /// Get detailed current trip information
    /// </summary>
    private async Task<CurrentTripInfo?> GetDetailedCurrentTripInfo(string truckId)
    {
        // Check for active cargo orders first
        var activeCargoOrder = await _context.Set<CargoOrders>()
            .Include(co => co.CargoOwner)
            .Where(co => co.AcceptedBid != null &&
                       co.AcceptedBid.TruckId == truckId &&
                       (co.Status == CargoOrderStatus.InTransit ||
                        co.Status == CargoOrderStatus.ReadyForPickup ||
                        co.Status == CargoOrderStatus.DriverAcknowledged))
            .FirstOrDefaultAsync();

        if (activeCargoOrder != null)
        {
            return new CurrentTripInfo
            {
                OrderId = activeCargoOrder.Id,
                OrderType = "Cargo",
                Status = activeCargoOrder.Status.ToString(),
                PickupLocation = activeCargoOrder.PickupLocation,
                DeliveryLocation = activeCargoOrder.DeliveryLocation,
                StartDate = activeCargoOrder.PickupDateTime,
                ExpectedDeliveryDate = activeCargoOrder.DeliveryDateTime,
                CargoType = activeCargoOrder.Items?.FirstOrDefault()?.Type.ToString() ?? "Mixed",
                CustomerName = activeCargoOrder.CargoOwner?.Name
            };
        }

        // Check for active normal orders
        var activeNormalOrder = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Routes)
            .Where(o => o.TruckId == truckId &&
                       (o.OrderStatus == OrderStatus.InTransit ||
                        o.OrderStatus == OrderStatus.Loaded ||
                        o.OrderStatus == OrderStatus.Assigned))
            .FirstOrDefaultAsync();

        if (activeNormalOrder != null)
        {
            return new CurrentTripInfo
            {
                OrderId = activeNormalOrder.Id,
                OrderType = "Normal",
                Status = activeNormalOrder.OrderStatus.ToString(),
                PickupLocation = activeNormalOrder.Routes?.FromRoute,
                DeliveryLocation = activeNormalOrder.DeliveryAddress ?? activeNormalOrder.Routes?.ToRoute,
                StartDate = activeNormalOrder.StartDate,
                ExpectedDeliveryDate = activeNormalOrder.EndDate,
                CargoType = activeNormalOrder.CargoType,
                CustomerName = activeNormalOrder.Customer?.CustomerName
            };
        }

        return null;
    }

    /// <summary>
    /// Check if truck is currently on a trip
    /// </summary>
    private async Task<bool> IsCurrentlyOnTrip(string truckId)
    {
        // Check cargo orders
        var hasActiveCargoOrder = await _context.Set<CargoOrders>()
            .AnyAsync(co => co.AcceptedBid != null &&
                          co.AcceptedBid.TruckId == truckId &&
                          (co.Status == CargoOrderStatus.InTransit ||
                           co.Status == CargoOrderStatus.ReadyForPickup ||
                           co.Status == CargoOrderStatus.DriverAcknowledged));

        if (hasActiveCargoOrder) return true;

        // Check normal orders
        var hasActiveNormalOrder = await _context.Orders
            .AnyAsync(o => o.TruckId == truckId &&
                          (o.OrderStatus == OrderStatus.InTransit ||
                           o.OrderStatus == OrderStatus.Loaded ||
                           o.OrderStatus == OrderStatus.Assigned));

        return hasActiveNormalOrder;
    }

    /// <summary>
    /// Generate CSV data from truck collection
    /// </summary>
    private byte[] GenerateCsvData(IEnumerable<EnhancedTruckResponseModel> trucks)
    {
        var csvBuilder = new StringBuilder();

        // Add CSV headers
        csvBuilder.AppendLine("Id,PlateNumber,TruckName,TruckType,TruckCapacity,TruckStatus,ApprovalStatus," +
                            "TruckLicenseExpiryDate,InsuranceExpiryDate,DriverName,DriverPhone,DriverEmail," +
                            "TruckOwnerName,IsDriverOwned,TotalCargoOrders,TotalNormalOrders," +
                            "IsCurrentlyOnTrip,CurrentTripType,CreatedAt");

        // Add data rows
        foreach (var truck in trucks)
        {
            var csvModel = TruckCsvExportModel.FromEnhancedModel(truck);
            csvBuilder.AppendLine($"{EscapeCsvValue(csvModel.Id)}," +
                                $"{EscapeCsvValue(csvModel.PlateNumber)}," +
                                $"{EscapeCsvValue(csvModel.TruckName)}," +
                                $"{EscapeCsvValue(csvModel.TruckType)}," +
                                $"{EscapeCsvValue(csvModel.TruckCapacity)}," +
                                $"{EscapeCsvValue(csvModel.TruckStatus)}," +
                                $"{EscapeCsvValue(csvModel.ApprovalStatus)}," +
                                $"{EscapeCsvValue(csvModel.TruckLicenseExpiryDate)}," +
                                $"{EscapeCsvValue(csvModel.InsuranceExpiryDate)}," +
                                $"{EscapeCsvValue(csvModel.DriverName)}," +
                                $"{EscapeCsvValue(csvModel.DriverPhone)}," +
                                $"{EscapeCsvValue(csvModel.DriverEmail)}," +
                                $"{EscapeCsvValue(csvModel.TruckOwnerName)}," +
                                $"{EscapeCsvValue(csvModel.IsDriverOwned)}," +
                                $"{csvModel.TotalCargoOrders}," +
                                $"{csvModel.TotalNormalOrders}," +
                                $"{EscapeCsvValue(csvModel.IsCurrentlyOnTrip)}," +
                                $"{EscapeCsvValue(csvModel.CurrentTripType)}," +
                                $"{EscapeCsvValue(csvModel.CreatedAt)}");
        }

        return Encoding.UTF8.GetBytes(csvBuilder.ToString());
    }

    /// <summary>
    /// Escape CSV values to handle commas, quotes, and newlines
    /// </summary>
    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

}