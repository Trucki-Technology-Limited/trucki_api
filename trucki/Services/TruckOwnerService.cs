using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class TruckOwnerService: ITruckOwnerService
{
    private readonly ITruckOwnerRepository _ownerRepository;
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IDriverService _driverService;
    private readonly INotificationService _notificationService;
    private readonly ITruckService _truckService;

    public TruckOwnerService(
        ITruckOwnerRepository ownerRepository,
        TruckiDBContext context,
        UserManager<User> userManager,
        IDriverService driverService,
        INotificationService notificationService,
        ITruckService truckService)
    {
        _ownerRepository = ownerRepository;
        _context = context;
        _userManager = userManager;
        _driverService = driverService;
        _notificationService = notificationService;
        _truckService = truckService;
    }
    


    public async Task<ApiResponseModel<bool>> CreateNewTruckOwner(AddTruckOwnerRequestBody model)
    {
        var res = await _ownerRepository.CreateNewTruckOwner(model);
        return res;
    }

    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetTruckOwnerById(string id)
    {
        var res = await _ownerRepository.GetTruckOwnerById(id);
        return res;
    }

    public async Task<ApiResponseModel<bool>> EditTruckOwner(EditTruckOwnerRequestBody model)
    {
        var res = await _ownerRepository.EditTruckOwner(model);
        return res;
    }

    public async Task<ApiResponseModel<bool>> DeleteTruckOwner(string id)
    {
        var res = await _ownerRepository.DeleteTruckOwner(id);
        return res;
    }

    public async Task<ApiResponseModel<List<AllTruckOwnerResponseModel>>> GetAllTruckOwners()
    {
        var res = await _ownerRepository.GetAllTruckOwners();
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>> SearchTruckOwners(string searchWords)
    {
        var res = await _ownerRepository.SearchTruckOwners(searchWords);
        return res;
    }
      public async Task<ApiResponseModel<bool>> AddNewTransporter(AddTransporterRequestBody model)
    {
        var res = await _ownerRepository.AddNewTransporter(model);
        return res;
    }
    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetTransporterProfileById(string id)
    {
        var res = await _ownerRepository.GetTransporterProfileById(id);
        return res;
    }
     // New methods for managing TruckOwner status
    public async Task<ApiResponseModel<bool>> ApproveTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.ApproveTruckOwner(truckOwnerId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> NotApproveTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.NotApproveTruckOwner(truckOwnerId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> BlockTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.BlockTruckOwner(truckOwnerId);
        return res;
    }

    public async Task<ApiResponseModel<bool>> UnblockTruckOwner(string truckOwnerId)
    {
        var res = await _ownerRepository.UnblockTruckOwner(truckOwnerId);
        return res;
    }

     public async Task<ApiResponseModel<bool>> UploadIdCardAndProfilePicture(string truckOwnerId, string idCardUrl, string profilePictureUrl)
    {
        var res = await _ownerRepository.UploadIdCardAndProfilePicture(truckOwnerId, idCardUrl, profilePictureUrl);
        return res;
    }

    public async Task<ApiResponseModel<bool>> UpdateBankDetails(UpdateBankDetailsRequestBody model)
    {
        var res = await _ownerRepository.UpdateBankDetails(model.Id, model);
        return res;
    }

    // DISPATCHER METHODS

    // UNIFIED FLEET OWNER REGISTRATION (Dispatcher + Transporter)
    public async Task<ApiResponseModel<bool>> RegisterFleetOwner(AddDispatcherRequestBody model)
    {
        try
        {
            // Determine role based on country
            string role = model.Country.ToUpper() == "US" ? "dispatcher" : "transporter";
            TruckOwnerType ownerType = model.Country.ToUpper() == "US" ? TruckOwnerType.Dispatcher : TruckOwnerType.Transporter;
            bool canBidOnBehalf = model.Country.ToUpper() == "US"; // Only US dispatchers can bid on behalf

            // Create user account
            var user = new User
            {
                UserName = model.EmailAddress,
                Email = model.EmailAddress,
                firstName = model.Name.Split(' ').FirstOrDefault() ?? model.Name,
                lastName = model.Name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                EmailConfirmed = false,
                PhoneNumber = model.Phone,
                IsActive = true,
                NormalizedUserName = model.EmailAddress.ToUpper(),
                NormalizedEmail = model.EmailAddress.ToUpper(),
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return ApiResponseModel<bool>.Fail($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}",400);
            }

            // Assign role based on country
            await _userManager.AddToRoleAsync(user, role);

            // Create TruckOwner entity
            var fleetOwner = new TruckOwner
            {
                Name = model.Name,
                EmailAddress = model.EmailAddress,
                Phone = model.Phone,
                Address = model.Address,
                IdCardUrl = model.IdCard,
                ProfilePictureUrl = model.ProfilePicture,
                UserId = user.Id,
                OwnersStatus = OwnersStatus.Pending,
                OwnerType = ownerType,
                Country = model.Country,
                CanBidOnBehalf = canBidOnBehalf
            };

            // Handle bank details if provided
            if (!string.IsNullOrEmpty(model.BankName))
            {
                var bankDetails = new BankDetails
                {
                    BankName = model.BankName,
                    BankAccountNumber = model.BankAccountNumber,
                    BankAccountName = model.BankAccountName
                };

                _context.BankDetails.Add(bankDetails);
                await _context.SaveChangesAsync();
                fleetOwner.BankDetailsId = bankDetails.Id;
            }

            _context.TruckOwners.Add(fleetOwner);
            await _context.SaveChangesAsync();

            string registrationType = model.Country.ToUpper() == "US" ? "Dispatcher" : "Transporter";
            return ApiResponseModel<bool>.Success($"{registrationType} registered successfully", true, 201);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error creating account: {ex.Message}",500);
        }
    }

    // UNIFIED PROFILE METHOD (works for both Dispatcher and Transporter)
    public async Task<ApiResponseModel<TruckOwnerResponseModel>> GetFleetOwnerProfile(string userId)
    {
        var res = await _ownerRepository.GetTransporterProfileById(userId);
        return res;
    }

    public async Task<ApiResponseModel<List<DriverResponseModel>>> GetDispatcherDrivers(string dispatcherId)
    {
        try
        {
            var drivers = await _context.Drivers
                .Include(d => d.Truck)
                .Include(d => d.BankAccounts)
                .Include(d => d.CommissionStructures)
                .Where(d => d.ManagedByDispatcherId == dispatcherId)
                .Select(d => new DriverResponseModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Phone = d.Phone,
                    EmailAddress = d.EmailAddress,
                    TruckId = d.TruckId,
                    UserId = d.UserId,
                    DriversLicence = d.DriversLicence,
                    DotNumber = d.DotNumber,
                    McNumber = d.McNumber,
                    PassportFile = d.PassportFile,
                    Country = d.Country,
                    IsActive = d.IsActive,
                    OnboardingStatus = d.OnboardingStatus,
                    HasAcceptedTerms = d.HasAcceptedLatestTerms
                })
                .ToListAsync();

            return ApiResponseModel<List<DriverResponseModel>>.Success("drivers retrieved successfully",drivers,200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<List<DriverResponseModel>>.Fail($"Error retrieving dispatcher drivers: {ex.Message}",500);
        }
    }

    public async Task<ApiResponseModel<string>> AddDriverToDispatcher(AddDriverForDispatcherRequestModel model)
    {
        try
        {
            // Get dispatcher to inherit country
            var dispatcher = await _context.TruckOwners
                .FirstOrDefaultAsync(d => d.Id == model.DispatcherId && d.OwnerType == TruckOwnerType.Dispatcher);

            if (dispatcher == null)
            {
                return ApiResponseModel<string>.Fail("Dispatcher not found", 404);
            }

            // Create user account for driver
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                firstName = model.Name.Split(' ').FirstOrDefault() ?? model.Name,
                lastName = model.Name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                EmailConfirmed = true,
                IsActive = true
            };

            var userResult = await _userManager.CreateAsync(user, model.Password);
            if (!userResult.Succeeded)
            {
                return ApiResponseModel<string>.Fail($"Failed to create user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}",400);
            }

            // Assign driver role
            await _userManager.AddToRoleAsync(user, "driver");

            // Create driver with minimal info (onboarding pending)
            var driver = new Driver
            {
                Name = model.Name,
                EmailAddress = model.Email,
                Phone = model.Number,
                UserId = user.Id,

                // Inherit country from dispatcher
                Country = dispatcher.Country,
                TruckOwnerId = model.DispatcherId,
                ManagedByDispatcherId = model.DispatcherId,
                OwnershipType = DriverOwnershipType.DispatcherManaged,
                OnboardingStatus = DriverOnboardingStatus.OboardingPending
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            // Create commission structure
            var commission = new DriverDispatcherCommission
            {
                DriverId = driver.Id,
                DispatcherId = model.DispatcherId,
                CommissionPercentage = model.CommissionPercentage,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            _context.DriverDispatcherCommissions.Add(commission);
            await _context.SaveChangesAsync();

            return ApiResponseModel<string>.Success("Driver created successfully. Complete onboarding by uploading documents and adding truck.",200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<string>.Fail($"Error adding driver: {ex.Message}",500);
        }
    }

    public async Task<ApiResponseModel<bool>> UploadDriverDocuments(UploadDriverDocumentsForDispatcherDto model)
    {
        try
        {
            // Validate dispatcher owns this driver
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == model.DriverId && d.ManagedByDispatcherId == model.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher",400);
            }

            // Upload documents using existing driver document functionality
            foreach (var doc in model.Documents)
            {
                var driverDocument = new DriverDocument
                {
                    DriverId = model.DriverId,
                    DocumentTypeId = doc.DocumentTypeId,
                    FileUrl = doc.FileUrl
                };

                _context.DriverDocuments.Add(driverDocument);
            }

            // Update driver with additional info
            if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
            {
                driver.PassportFile = model.ProfilePictureUrl;
            }

            if (!string.IsNullOrEmpty(model.DotNumber))
            {
                driver.DotNumber = model.DotNumber;
            }

            if (!string.IsNullOrEmpty(model.McNumber))
            {
                driver.McNumber = model.McNumber;
            }

            driver.OnboardingStatus = DriverOnboardingStatus.OnboardingInReview;
            await _context.SaveChangesAsync();

            return ApiResponseModel<bool>.Success("Driver documents uploaded successfully", true, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error uploading documents: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> AddTruckForDriver(AddTruckForDispatcherDriverDto model)
    {
        try
        {
            // Validate dispatcher owns this driver
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == model.DriverId && d.ManagedByDispatcherId == model.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher", 404);
            }

            // Map to DriverAddTruckRequestModel and use existing truck service
            var driverTruckModel = new DriverAddTruckRequestModel
            {
                DriverId = model.DriverId,
                PlateNumber = model.PlateNumber,
                TruckName = model.TruckName,
                TruckCapacity = model.TruckCapacity,
                TruckType = model.TruckType,
                TruckLicenseExpiryDate = model.TruckLicenseExpiryDate,
                RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate,
                InsuranceExpiryDate = model.InsuranceExpiryDate,
                Documents = model.Documents ?? new List<string>(),
                ExternalTruckPictureUrl = model.ExternalTruckPictureUrl,
                CargoSpacePictureUrl = model.CargoSpacePictureUrl
            };

            // Use existing truck service
            var truckResult = await _truckService.AddDriverOwnedTruck(driverTruckModel);
            var result = truckResult.IsSuccessful ?
                ApiResponseModel<bool>.Success("Truck added successfully", true, 200) :
                ApiResponseModel<bool>.Fail("Failed to add truck", 500);

            if (result.IsSuccessful)
            {
                // Update driver onboarding status if truck added successfully
                driver.OnboardingStatus = DriverOnboardingStatus.OnboardingCompleted;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error adding truck: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> CompleteDriverOnboarding(CompleteDriverOnboardingDto model)
    {
        try
        {
            var driver = await _context.Drivers
                .Include(d => d.Truck)
                .Include(d => d.DriverDocuments)
                .FirstOrDefaultAsync(d => d.Id == model.DriverId && d.ManagedByDispatcherId == model.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher", 404);
            }

            // Validate onboarding completion
            var hasRequiredDocuments = await ValidateRequiredDocuments(driver.Id, driver.Country);
            var hasTruck = driver.Truck != null;

            if (!hasRequiredDocuments)
            {
                return ApiResponseModel<bool>.Fail("Required documents not uploaded", 400);
            }

            if (!hasTruck)
            {
                return ApiResponseModel<bool>.Fail("Truck not added for driver", 400);
            }

            // Submit for admin approval
            driver.OnboardingStatus = DriverOnboardingStatus.OnboardingInReview;
            await _context.SaveChangesAsync();

            // Notify admin for approval
            // Note: You might need to implement this method in INotificationService
            // await _notificationService.NotifyAdminForDriverApproval(driver.Id, model.DispatcherId);

            return ApiResponseModel<bool>.Success("Driver onboarding completed and submitted for admin approval", true, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error completing onboarding: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> SetOrUpdateDriverCommission(string driverId, string dispatcherId, decimal newPercentage)
    {
        try
        {
            // Validate commission percentage
            if (newPercentage < 0 || newPercentage > 50)
            {
                return ApiResponseModel<bool>.Fail("Commission percentage must be between 0 and 50", 400);
            }

            // Verify driver exists and is managed by this dispatcher
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == driverId && d.ManagedByDispatcherId == dispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher", 404);
            }

            // Find active commission structure
            var activeCommission = await _context.DriverDispatcherCommissions
                .FirstOrDefaultAsync(c => c.DriverId == driverId
                                        && c.DispatcherId == dispatcherId
                                        && c.IsActive);

            if (activeCommission != null)
            {
                // End current commission structure
                activeCommission.IsActive = false;
                activeCommission.EffectiveTo = DateTime.UtcNow;
            }

            // Create new commission structure
            var newCommission = new DriverDispatcherCommission
            {
                DriverId = driverId,
                DispatcherId = dispatcherId,
                CommissionPercentage = newPercentage,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            _context.DriverDispatcherCommissions.Add(newCommission);
            await _context.SaveChangesAsync();

            string message = activeCommission != null
                ? "Commission updated successfully"
                : "Commission set successfully";

            return ApiResponseModel<bool>.Success(message, true, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error setting/updating commission: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<DriverCommissionHistoryResponseModel>> GetDriverCommissionHistory(string driverId, string dispatcherId)
    {
        try
        {
            // Verify driver exists and is managed by this dispatcher
            var driver = await _context.Drivers
                .Include(d => d.CommissionStructures)
                    .ThenInclude(c => c.Dispatcher)
                .FirstOrDefaultAsync(d => d.Id == driverId && d.ManagedByDispatcherId == dispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<DriverCommissionHistoryResponseModel>.Fail(
                    "Driver not found or not managed by this dispatcher", 404);
            }

            // Get all commission records for this driver-dispatcher pair
            var commissions = await _context.DriverDispatcherCommissions
                .Include(c => c.Driver)
                .Include(c => c.Dispatcher)
                .Where(c => c.DriverId == driverId && c.DispatcherId == dispatcherId)
                .OrderByDescending(c => c.EffectiveFrom)
                .ToListAsync();

            if (!commissions.Any())
            {
                return ApiResponseModel<DriverCommissionHistoryResponseModel>.Fail(
                    "No commission records found for this driver", 404);
            }

            var currentCommission = commissions.FirstOrDefault(c => c.IsActive);
            var dispatcher = commissions.First().Dispatcher;

            var response = new DriverCommissionHistoryResponseModel
            {
                DriverId = driverId,
                DriverName = driver.Name,
                DispatcherId = dispatcherId,
                CurrentCommission = currentCommission != null ? new DriverCommissionResponseModel
                {
                    Id = currentCommission.Id,
                    DriverId = currentCommission.DriverId,
                    DriverName = driver.Name,
                    DispatcherId = currentCommission.DispatcherId,
                    DispatcherName = dispatcher.Name,
                    CommissionPercentage = currentCommission.CommissionPercentage,
                    EffectiveFrom = currentCommission.EffectiveFrom,
                    EffectiveTo = currentCommission.EffectiveTo,
                    IsActive = currentCommission.IsActive
                } : null,
                CommissionHistory = commissions.Select(c => new DriverCommissionResponseModel
                {
                    Id = c.Id,
                    DriverId = c.DriverId,
                    DriverName = driver.Name,
                    DispatcherId = c.DispatcherId,
                    DispatcherName = dispatcher.Name,
                    CommissionPercentage = c.CommissionPercentage,
                    EffectiveFrom = c.EffectiveFrom,
                    EffectiveTo = c.EffectiveTo,
                    IsActive = c.IsActive
                }).ToList()
            };

            return ApiResponseModel<DriverCommissionHistoryResponseModel>.Success(
                "Commission history retrieved successfully", response, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<DriverCommissionHistoryResponseModel>.Fail(
                $"Error retrieving commission history: {ex.Message}", 500);
        }
    }

    private async Task<bool> ValidateRequiredDocuments(string driverId, string country)
    {
        // Get required document types for this country and entity type
        var requiredDocTypes = await _context.DocumentTypes
            .Where(dt => dt.Country == country && dt.EntityType == "Driver" && dt.IsRequired)
            .Select(dt => dt.Id)
            .ToListAsync();

        var uploadedDocTypes = await _context.DriverDocuments
            .Where(dd => dd.DriverId == driverId)
            .Select(dd => dd.DocumentTypeId)
            .ToListAsync();

        // Check if all required documents are uploaded
        return requiredDocTypes.All(rdt => uploadedDocTypes.Contains(rdt));
    }
}