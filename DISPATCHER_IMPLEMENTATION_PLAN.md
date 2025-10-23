# Dispatcher Implementation Plan

## Executive Summary

This document outlines the implementation plan for extending the existing TruckOwner entity to accommodate US market dispatchers who can manage drivers and bid on CargoOrders on their behalf, similar to the existing transporter functionality in the Nigerian market.

## Current System Analysis

### Existing Architecture
- **TruckOwner Entity**: Used for both truck owners and transporters (differentiated by user role)
- **Order vs CargoOrder**: Orders for Nigerian market, CargoOrders for US market with bidding system
- **Driver Management**: TruckOwners can add drivers, who go through admin approval
- **Bidding System**: Direct driver bidding on CargoOrders with admin approval workflow

### Key Entities
- `TruckOwner`: Main entity for fleet management
- `Driver`: Individual drivers with truck assignments
- `CargoOrders`: US market orders with bidding system
- `Bid`: Links drivers to orders with amounts
- `User`: Identity management with role-based access

## Implementation Strategy

### 1. Extend TruckOwner Entity for Dispatchers

#### 1.1 Add Dispatcher-Specific Properties
```csharp
public class TruckOwner : BaseClass
{
    // Existing properties...

    // New properties for dispatcher functionality
    public TruckOwnerType OwnerType { get; set; } = TruckOwnerType.TruckOwner;
    public string Country { get; set; } = "NG"; // Country code (NG, US, etc.)
    public bool CanBidOnBehalf { get; set; } = false; // For dispatchers
}

public enum TruckOwnerType
{
    TruckOwner,      // Traditional truck owner
    Transporter,     // Nigerian market transporter
    Dispatcher       // US market dispatcher
}
```

#### 1.2 Driver-Dispatcher Commission Structure
```csharp
public class DriverDispatcherCommission : BaseClass
{
    public string DriverId { get; set; }
    public Driver Driver { get; set; }
    public string DispatcherId { get; set; } // TruckOwner ID
    public TruckOwner Dispatcher { get; set; }
    public decimal CommissionPercentage { get; set; } // Dispatcher's cut (e.g., 15%)
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 2. Enhance Driver Management

#### 2.1 Modify Driver Entity
```csharp
public class Driver : BaseClass
{
    // Existing properties...
    // Note: Country property already exists in Driver entity

    // Enhanced relationship properties
    public DriverOwnershipType OwnershipType { get; set; } = DriverOwnershipType.Independent;
    public string? ManagedByDispatcherId { get; set; } // For dispatcher-managed drivers
    public TruckOwner? ManagedByDispatcher { get; set; }

    // Commission settings
    public ICollection<DriverDispatcherCommission> CommissionStructures { get; set; } = new List<DriverDispatcherCommission>();
}

public enum DriverOwnershipType
{
    Independent,           // Driver owns their truck
    TruckOwnerEmployee,   // Employee of truck owner
    DispatcherManaged     // Managed by dispatcher
}
```

#### 2.2 Enhanced Add Driver Request (Initial Creation)
```csharp
public class AddDriverForDispatcherRequestModel
{
    // Basic driver info (minimal for initial creation)
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string address { set; get; }
    public string Password { set; get; } // For driver login

    // Dispatcher-specific fields
    public string DispatcherId { get; set; }
    public decimal CommissionPercentage { get; set; } // 0-50% range
    public bool IsDispatcherManaged { get; set; } = true;

    // Country will be inherited from dispatcher
    // Documents and truck details will be handled in separate onboarding steps
}
```

#### 2.3 Driver Onboarding Process for Dispatchers
```csharp
// Step 1: Upload Driver Documents (Using existing system)
public class UploadDriverDocumentsForDispatcherDto
{
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }

    // Using existing CreateDriverDocumentRequest structure
    public List<CreateDriverDocumentRequest> Documents { get; set; } = new();

    // Additional basic driver info if not provided during initial creation
    public string? ProfilePictureUrl { get; set; }
    public string? DotNumber { get; set; } // Required for US drivers
    public string? McNumber { get; set; } // Optional for US drivers
}

// Step 2: Add Truck for Driver (Using existing DriverAddTruckRequestModel)
public class AddTruckForDispatcherDriverDto : DriverAddTruckRequestModel
{
    public string DispatcherId { get; set; }
    // Inherits all existing truck fields:
    // - PlateNumber, TruckName, TruckCapacity, TruckType
    // - TruckLicenseExpiryDate, RoadWorthinessExpiryDate, InsuranceExpiryDate
    // - Documents, ExternalTruckPictureUrl, CargoSpacePictureUrl
}

// Step 3: Complete Onboarding
public class CompleteDriverOnboardingDto
{
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }
    public bool ConfirmAllDocumentsUploaded { get; set; }
    public bool ConfirmTruckAdded { get; set; }
}
```

### 3. Bidding System Enhancements

#### 3.1 Modify Bid Entity
```csharp
public class Bid : BaseClass
{
    // Existing properties...
    public string OrderId { get; set; }
    public CargoOrders Order { get; set; }
    public string TruckId { get; set; }
    public Truck Truck { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public DateTime? DriverAcknowledgedAt { get; set; }

    // Enhanced bidding properties for dispatcher functionality
    public BidSubmitterType SubmitterType { get; set; } = BidSubmitterType.Driver;
    public string? SubmittedByDispatcherId { get; set; } // If bid by dispatcher
    public TruckOwner? SubmittedByDispatcher { get; set; }
    public decimal? DispatcherCommissionAmount { get; set; } // Calculated commission

    // Important: The bid always references the actual driver/truck
    // CargoOwner sees only the driver, Admin sees both dispatcher and driver
}

public enum BidSubmitterType
{
    Driver,      // Direct driver bid
    Dispatcher   // Bid submitted by dispatcher on behalf of driver
}
```

#### 3.2 Enhanced Bid Creation
```csharp
public class CreateBidOnBehalfDto
{
    public string OrderId { get; set; }
    public string DriverId { get; set; }  // Selected driver for the bid
    public string DispatcherId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

// Document upload DTOs (similar to existing driver document upload)
public class UploadManifestOnBehalfDto
{
    public string OrderId { get; set; }
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }  // Verify dispatcher manages this driver
    public List<string> ManifestUrls { get; set; }
}

public class CompleteDeliveryOnBehalfDto
{
    public string OrderId { get; set; }
    public string DispatcherId { get; set; }
    public string DriverId { get; set; }  // Verify dispatcher manages this driver
    public List<string> DeliveryDocumentUrls { get; set; }
}
```

#### 3.3 Bid Response Models for Different User Types
```csharp
// For CargoOwner - they see driver information with dispatcher indicator
public class CargoOwnerBidResponseModel
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public string DriverId { get; set; }
    public string DriverName { get; set; }
    public string TruckId { get; set; }
    public TruckResponseModel Truck { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsFromDispatcher { get; set; } // Simple indicator for cargo owner
}

// For Admin - they see both driver and dispatcher information
public class AdminBidResponseModel
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public string DriverId { get; set; }
    public string DriverName { get; set; }
    public string TruckId { get; set; }
    public TruckResponseModel Truck { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }

    // Additional dispatcher information for admin
    public BidSubmitterType SubmitterType { get; set; }
    public string? SubmittedByDispatcherId { get; set; }
    public string? DispatcherName { get; set; }
    public decimal? DispatcherCommissionAmount { get; set; }
}
```

### 4. User Role System Updates

#### 4.1 Add Dispatcher Role
```csharp
// In SeedData.cs, add new role
var roles = new[] {
    "admin", "finance", "chiefmanager", "manager", "driver",
    "cargo owner", "transporter", "dispatcher", "hr",
    "field officer", "safety officer"
};
```

#### 4.2 Registration Enhancement
```csharp
public class AddDispatcherRequestBody
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Password { get; set; }
    public string Country { get; set; } = "US"; // Country selection (US, NG, etc.)
    public string IdCard { get; set; }
    public string ProfilePicture { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
}
```

### 5. Service Layer Enhancements

#### 5.1 TruckOwnerService Updates
```csharp
public interface ITruckOwnerService
{
    // Existing methods...

    // New dispatcher methods
    Task<ApiResponseModel<bool>> AddNewDispatcher(AddDispatcherRequestBody model);
    Task<ApiResponseModel<TruckOwnerResponseModel>> GetDispatcherProfileById(string dispatcherId);
    Task<ApiResponseModel<List<DriverResponseModel>>> GetDispatcherDrivers(string dispatcherId);

    // Driver onboarding methods
    Task<ApiResponseModel<string>> AddDriverToDispatcher(AddDriverForDispatcherRequestModel model);
    Task<ApiResponseModel<bool>> UploadDriverDocuments(UploadDriverDocumentsForDispatcherDto model);
    Task<ApiResponseModel<bool>> AddTruckForDriver(AddTruckForDispatcherDriverDto model);
    Task<ApiResponseModel<bool>> CompleteDriverOnboarding(CompleteDriverOnboardingDto model);

    // Commission management
    Task<ApiResponseModel<bool>> UpdateDriverCommission(string driverId, string dispatcherId, decimal newPercentage);
}
```

#### 5.2 Service Implementation Logic for Driver Onboarding
```csharp
public class TruckOwnerService : ITruckOwnerService
{
    // Step 1: Initial driver creation
    public async Task<ApiResponseModel<string>> AddDriverToDispatcher(AddDriverForDispatcherRequestModel model)
    {
        try
        {
            // Get dispatcher to inherit country
            var dispatcher = await _context.TruckOwners
                .FirstOrDefaultAsync(d => d.Id == model.DispatcherId);

            if (dispatcher == null)
            {
                return ApiResponseModel<string>.Fail("Dispatcher not found", 404);
            }

            // Create user account for driver
            var user = await _userService.CreateUserAsync(model.Email, model.Password, "driver");

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

            // Create commission structure
            var commission = new DriverDispatcherCommission
            {
                DriverId = driver.Id,
                DispatcherId = model.DispatcherId,
                CommissionPercentage = model.CommissionPercentage,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            _context.Drivers.Add(driver);
            _context.DriverDispatcherCommissions.Add(commission);
            await _context.SaveChangesAsync();

            return ApiResponseModel<string>.Success(driver.Id, "Driver created successfully. Complete onboarding by uploading documents and adding truck.");
        }
        catch (Exception ex)
        {
            return ApiResponseModel<string>.Fail($"Error adding driver: {ex.Message}");
        }
    }

    // Step 2: Upload driver documents
    public async Task<ApiResponseModel<bool>> UploadDriverDocuments(UploadDriverDocumentsForDispatcherDto model)
    {
        try
        {
            // Validate dispatcher owns this driver
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == model.DriverId && d.ManagedByDispatcherId == model.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher");
            }

            // Upload documents using existing service
            foreach (var doc in model.Documents)
            {
                doc.DriverId = model.DriverId; // Ensure correct driver ID
                await _driverDocumentService.CreateAsync(doc);
            }

            // Update driver with additional info
            if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
            {
                driver.PassportFile = model.ProfilePictureUrl; // Using existing field
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

            return ApiResponseModel<bool>.Success(true, "Driver documents uploaded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error uploading documents: {ex.Message}");
        }
    }

    // Step 3: Add truck for driver
    public async Task<ApiResponseModel<bool>> AddTruckForDriver(AddTruckForDispatcherDriverDto model)
    {
        try
        {
            // Validate dispatcher owns this driver
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == model.DriverId && d.ManagedByDispatcherId == model.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher");
            }

            // Use existing driver truck service but ensure dispatcher authorization
            var result = await _driverService.AddTruckAsync(model);

            if (result.IsSuccess)
            {
                // Update driver onboarding status if truck added successfully
                driver.OnboardingStatus = DriverOnboardingStatus.OnboardingCompleted;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error adding truck: {ex.Message}");
        }
    }

    // Step 4: Complete onboarding
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
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher");
            }

            // Validate onboarding completion
            var hasRequiredDocuments = await ValidateRequiredDocuments(driver.Id, driver.Country);
            var hasTruck = driver.Truck != null;

            if (!hasRequiredDocuments)
            {
                return ApiResponseModel<bool>.Fail("Required documents not uploaded");
            }

            if (!hasTruck)
            {
                return ApiResponseModel<bool>.Fail("Truck not added for driver");
            }

            // Submit for admin approval
            driver.OnboardingStatus = DriverOnboardingStatus.OnboardingInReview;
            await _context.SaveChangesAsync();

            // Notify admin for approval
            await _notificationService.NotifyAdminForDriverApproval(driver.Id, model.DispatcherId);

            return ApiResponseModel<bool>.Success(true, "Driver onboarding completed and submitted for admin approval");
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error completing onboarding: {ex.Message}");
        }
    }

    private async Task<bool> ValidateRequiredDocuments(string driverId, string country)
    {
        // Get required document types for this country and entity type
        var requiredDocTypes = await _context.DocumentTypes
            .Where(dt => dt.Country == country && dt.EntityType == "Driver" && dt.IsRequired)
            .ToListAsync();

        var uploadedDocTypes = await _context.DriverDocuments
            .Where(dd => dd.DriverId == driverId)
            .Select(dd => dd.DocumentTypeId)
            .ToListAsync();

        // Check if all required documents are uploaded
        return requiredDocTypes.All(rdt => uploadedDocTypes.Contains(rdt.Id));
    }
}
```

#### 5.3 CargoOrderService Updates
```csharp
public interface ICargoOrderService
{
    // Existing methods...

    // New dispatcher bidding methods
    Task<ApiResponseModel<bool>> CreateBidOnBehalf(CreateBidOnBehalfDto createBidDto);
    Task<ApiResponseModel<List<CargoOrderResponseModel>>> GetAvailableOrdersForDispatcher(string dispatcherId);
    Task<ApiResponseModel<bool>> AssignOrderToDriverFromDispatcher(string orderId, string driverId, string dispatcherId);

    // Role-based bid retrieval methods
    Task<ApiResponseModel<List<CargoOwnerBidResponseModel>>> GetBidsForCargoOwner(string orderId, string cargoOwnerId);
    Task<ApiResponseModel<List<AdminBidResponseModel>>> GetBidsForAdmin(string orderId);

    // Dispatcher document upload methods (similar to driver flow)
    Task<ApiResponseModel<bool>> UploadManifestOnBehalf(UploadManifestOnBehalfDto dto);
    Task<ApiResponseModel<bool>> CompleteDeliveryOnBehalf(CompleteDeliveryOnBehalfDto dto);
}
```

#### 5.4 Service Implementation for Role-Based Bid Display
```csharp
public class CargoOrderService : ICargoOrderService
{
    public async Task<ApiResponseModel<bool>> CreateBidOnBehalf(CreateBidOnBehalfDto createBidDto)
    {
        try
        {
            // Validate dispatcher can bid for this driver
            var commission = await _context.DriverDispatcherCommissions
                .FirstOrDefaultAsync(c => c.DriverId == createBidDto.DriverId
                                       && c.DispatcherId == createBidDto.DispatcherId
                                       && c.IsActive);

            if (commission == null)
            {
                return ApiResponseModel<bool>.Fail("Dispatcher not authorized to bid for this driver");
            }

            var driver = await _context.Drivers
                .Include(d => d.Truck)
                .FirstOrDefaultAsync(d => d.Id == createBidDto.DriverId);

            if (driver?.Truck == null)
            {
                return ApiResponseModel<bool>.Fail("Driver or truck not found");
            }

            // Calculate dispatcher commission
            var systemFee = createBidDto.Amount * 0.20m;
            var netAmount = createBidDto.Amount - systemFee;
            var dispatcherCommission = netAmount * (commission.CommissionPercentage / 100);

            // Create bid - IMPORTANT: Bid references the driver, not the dispatcher
            var bid = new Bid
            {
                OrderId = createBidDto.OrderId,
                TruckId = driver.TruckId,
                Amount = createBidDto.Amount,
                Status = BidStatus.Pending,

                // Dispatcher tracking (hidden from cargo owner)
                SubmitterType = BidSubmitterType.Dispatcher,
                SubmittedByDispatcherId = createBidDto.DispatcherId,
                DispatcherCommissionAmount = dispatcherCommission,

                // Notes can include dispatcher info for admin
                Notes = $"Bid submitted by dispatcher on behalf of driver. {createBidDto.Notes ?? ""}"
            };

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            return ApiResponseModel<bool>.Success(true, "Bid created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error creating bid: {ex.Message}");
        }
    }

    public async Task<ApiResponseModel<List<CargoOwnerBidResponseModel>>> GetBidsForCargoOwner(string orderId, string cargoOwnerId)
    {
        // CargoOwner sees driver information with dispatcher indicator
        var bids = await _context.Bids
            .Include(b => b.Truck)
                .ThenInclude(t => t.Driver)
            .Where(b => b.OrderId == orderId)
            .Select(b => new CargoOwnerBidResponseModel
            {
                Id = b.Id,
                Amount = b.Amount,
                Status = b.Status,
                DriverId = b.Truck.Driver.Id,
                DriverName = b.Truck.Driver.Name,
                TruckId = b.TruckId,
                Truck = _mapper.Map<TruckResponseModel>(b.Truck),
                CreatedAt = b.CreatedAt,
                Notes = b.Notes?.Split("Bid submitted by dispatcher")[0].Trim(), // Remove dispatcher info from notes
                IsFromDispatcher = b.SubmitterType == BidSubmitterType.Dispatcher // Simple boolean indicator
            })
            .ToListAsync();

        return ApiResponseModel<List<CargoOwnerBidResponseModel>>.Success(bids);
    }

    public async Task<ApiResponseModel<List<AdminBidResponseModel>>> GetBidsForAdmin(string orderId)
    {
        // Admin sees full information including dispatcher details
        var bids = await _context.Bids
            .Include(b => b.Truck)
                .ThenInclude(t => t.Driver)
            .Include(b => b.SubmittedByDispatcher)
            .Where(b => b.OrderId == orderId)
            .Select(b => new AdminBidResponseModel
            {
                Id = b.Id,
                Amount = b.Amount,
                Status = b.Status,
                DriverId = b.Truck.Driver.Id,
                DriverName = b.Truck.Driver.Name,
                TruckId = b.TruckId,
                Truck = _mapper.Map<TruckResponseModel>(b.Truck),
                CreatedAt = b.CreatedAt,
                Notes = b.Notes,

                // Dispatcher information for admin
                SubmitterType = b.SubmitterType,
                SubmittedByDispatcherId = b.SubmittedByDispatcherId,
                DispatcherName = b.SubmittedByDispatcher?.Name,
                DispatcherCommissionAmount = b.DispatcherCommissionAmount
            })
            .ToListAsync();

        return ApiResponseModel<List<AdminBidResponseModel>>.Success(bids);
    }

    // Dispatcher document upload methods (similar to driver flow)
    public async Task<ApiResponseModel<bool>> UploadManifestOnBehalf(UploadManifestOnBehalfDto dto)
    {
        try
        {
            // Validate dispatcher manages this driver
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == dto.DriverId && d.ManagedByDispatcherId == dto.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher");
            }

            // Validate order belongs to this driver
            var order = await _context.CargoOrders
                .Include(o => o.AcceptedBid)
                    .ThenInclude(b => b.Truck)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId &&
                                        o.AcceptedBid != null &&
                                        o.AcceptedBid.Truck.DriverId == dto.DriverId);

            if (order == null)
            {
                return ApiResponseModel<bool>.Fail("Order not found or not assigned to this driver");
            }

            // Upload manifest documents (same logic as driver upload)
            order.Documents = dto.ManifestUrls;
            order.Status = CargoOrderStatus.InTransit; // Status change when manifest uploaded

            await _context.SaveChangesAsync();

            // Notify cargo owner
            await _notificationService.NotifyCargoOwnerManifestUploaded(order.CargoOwnerId, order.Id);

            return ApiResponseModel<bool>.Success(true, "Manifest uploaded successfully on behalf of driver");
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error uploading manifest: {ex.Message}");
        }
    }

    public async Task<ApiResponseModel<bool>> CompleteDeliveryOnBehalf(CompleteDeliveryOnBehalfDto dto)
    {
        try
        {
            // Validate dispatcher manages this driver
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == dto.DriverId && d.ManagedByDispatcherId == dto.DispatcherId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found or not managed by this dispatcher");
            }

            // Validate order belongs to this driver
            var order = await _context.CargoOrders
                .Include(o => o.AcceptedBid)
                    .ThenInclude(b => b.Truck)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId &&
                                        o.AcceptedBid != null &&
                                        o.AcceptedBid.Truck.DriverId == dto.DriverId);

            if (order == null)
            {
                return ApiResponseModel<bool>.Fail("Order not found or not assigned to this driver");
            }

            // Upload delivery documents and complete order
            order.DeliveryDocuments = dto.DeliveryDocumentUrls;
            order.Status = CargoOrderStatus.Delivered; // Status change when delivery docs uploaded

            await _context.SaveChangesAsync();

            // Process commission payments
            await _dispatcherPayoutService.ProcessOrderCompletion(order.Id);

            // Notify cargo owner and driver
            await _notificationService.NotifyCargoOwnerOrderCompleted(order.CargoOwnerId, order.Id);
            await _notificationService.NotifyDriverOrderCompleted(dto.DriverId, order.Id);

            return ApiResponseModel<bool>.Success(true, "Delivery completed successfully on behalf of driver");
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error completing delivery: {ex.Message}");
        }
    }
}
```

### 6. Financial Flow Implementation

#### 6.1 Commission Calculation Logic
```csharp
public class DispatcherFinancialService
{
    public async Task<CommissionCalculationResult> CalculateEarnings(string orderId, string driverId)
    {
        var order = await GetCargoOrderWithBid(orderId);
        var commission = await GetActiveCommissionStructure(driverId);

        var totalAmount = order.AcceptedBid.Amount;
        var systemFee = totalAmount * 0.20m; // 20% system fee
        var netAmount = totalAmount - systemFee;

        var dispatcherCut = netAmount * (commission.CommissionPercentage / 100);
        var driverEarnings = netAmount - dispatcherCut;

        return new CommissionCalculationResult
        {
            TotalBidAmount = totalAmount,
            SystemFee = systemFee,
            DispatcherCommission = dispatcherCut,
            DriverEarnings = driverEarnings
        };
    }
}
```

#### 6.2 Payout Service Updates
```csharp
public class DispatcherPayoutService
{
    public async Task ProcessOrderCompletion(string orderId)
    {
        var calculation = await _financialService.CalculateEarnings(orderId, driverId);

        // Update driver wallet
        await _driverWalletService.AddEarnings(driverId, calculation.DriverEarnings);

        // Update dispatcher wallet (new service)
        await _dispatcherWalletService.AddCommission(dispatcherId, calculation.DispatcherCommission);

        // Update order with breakdown
        await UpdateOrderFinancials(orderId, calculation);
    }
}
```

### 7. Database Migration Strategy

#### 7.1 Phase 1: Schema Updates
```sql
-- Add new columns to TruckOwner table
ALTER TABLE TruckOwners
ADD OwnerType int DEFAULT 0,
ADD Country nvarchar(10) DEFAULT 'NG',
ADD CanBidOnBehalf bit DEFAULT 0;

-- Create DriverDispatcherCommission table
CREATE TABLE DriverDispatcherCommissions (
    Id nvarchar(450) PRIMARY KEY,
    DriverId nvarchar(450) NOT NULL,
    DispatcherId nvarchar(450) NOT NULL,
    CommissionPercentage decimal(5,2) NOT NULL,
    EffectiveFrom datetime2 NOT NULL,
    EffectiveTo datetime2 NULL,
    IsActive bit DEFAULT 1,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NOT NULL,

    FOREIGN KEY (DriverId) REFERENCES Drivers(Id),
    FOREIGN KEY (DispatcherId) REFERENCES TruckOwners(Id)
);
```

#### 7.2 Phase 2: Data Migration
```csharp
public class MigrateExistingDataService
{
    public async Task MigrateExistingTruckOwners()
    {
        // Set existing truck owners as TruckOwner type
        var existingOwners = await _context.TruckOwners.ToListAsync();

        foreach (var owner in existingOwners)
        {
            // Determine type based on user role
            var userRole = await GetUserRole(owner.UserId);

            owner.OwnerType = userRole switch
            {
                "transporter" => TruckOwnerType.Transporter,
                "dispatcher" => TruckOwnerType.Dispatcher,
                _ => TruckOwnerType.TruckOwner
            };

            owner.Country = DetermineCountryFromContext(owner);
            owner.CanBidOnBehalf = userRole == "transporter" || userRole == "dispatcher";
        }

        await _context.SaveChangesAsync();
    }
}
```

### 8. API Endpoints Design

#### 8.1 Dispatcher Management Endpoints
```csharp
[ApiController]
[Route("api/[controller]")]
public class DispatcherController : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseModel<bool>>> RegisterDispatcher([FromBody] AddDispatcherRequestBody model)

    // Driver onboarding endpoints
    [HttpPost("{dispatcherId}/drivers")]
    public async Task<ActionResult<ApiResponseModel<string>>> AddDriver([FromBody] AddDriverForDispatcherRequestModel model)
    {
        var result = await _truckOwnerService.AddDriverToDispatcher(model);
        return Ok(result); // Returns driver ID for next steps
    }

    [HttpPost("{dispatcherId}/drivers/{driverId}/documents")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UploadDriverDocuments([FromBody] UploadDriverDocumentsForDispatcherDto model)
    {
        var result = await _truckOwnerService.UploadDriverDocuments(model);
        return Ok(result);
    }

    [HttpPost("{dispatcherId}/drivers/{driverId}/truck")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AddTruckForDriver([FromBody] AddTruckForDispatcherDriverDto model)
    {
        var result = await _truckOwnerService.AddTruckForDriver(model);
        return Ok(result);
    }

    [HttpPost("{dispatcherId}/drivers/{driverId}/complete-onboarding")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CompleteDriverOnboarding([FromBody] CompleteDriverOnboardingDto model)
    {
        var result = await _truckOwnerService.CompleteDriverOnboarding(model);
        return Ok(result);
    }

    // Driver management
    [HttpGet("{dispatcherId}/drivers")]
    public async Task<ActionResult<ApiResponseModel<List<DriverResponseModel>>>> GetDrivers(string dispatcherId)

    [HttpPut("drivers/{driverId}/commission")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UpdateCommission([FromBody] UpdateCommissionRequestModel model)

    // Bidding
    [HttpPost("bids")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CreateBidOnBehalf([FromBody] CreateBidOnBehalfDto model)

    [HttpGet("{dispatcherId}/orders/available")]
    public async Task<ActionResult<ApiResponseModel<List<CargoOrderResponseModel>>>> GetAvailableOrders(string dispatcherId)

    // Order document management (similar to driver flow)
    [HttpPost("orders/{orderId}/manifest")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UploadManifestOnBehalf([FromBody] UploadManifestOnBehalfDto model)
    {
        var result = await _cargoOrderService.UploadManifestOnBehalf(model);
        return Ok(result);
    }

    [HttpPost("orders/{orderId}/delivery")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CompleteDeliveryOnBehalf([FromBody] CompleteDeliveryOnBehalfDto model)
    {
        var result = await _cargoOrderService.CompleteDeliveryOnBehalf(model);
        return Ok(result);
    }

    // Get required document types for driver onboarding
    [HttpGet("document-types/{country}")]
    public async Task<ActionResult<ApiResponseModel<List<DocumentTypeResponseModel>>>> GetRequiredDocumentTypes(string country)
    {
        var result = await _documentTypeService.GetByCountryAndEntityType(country, "Driver");
        return Ok(result);
    }
}
```

#### 8.2 Role-Based Bid Viewing Endpoints
```csharp
[ApiController]
[Route("api/cargoorders")]
public class CargoOrderController : ControllerBase
{
    // CargoOwner endpoint - only sees driver information
    [HttpGet("{orderId}/bids")]
    [Authorize(Roles = "cargo owner")]
    public async Task<ActionResult<ApiResponseModel<List<CargoOwnerBidResponseModel>>>> GetBidsForCargoOwner(string orderId)
    {
        var cargoOwnerId = User.FindFirst("CargoOwnerId")?.Value;
        var result = await _cargoOrderService.GetBidsForCargoOwner(orderId, cargoOwnerId);
        return Ok(result);
    }

    // Admin endpoint - sees full information including dispatcher details
    [HttpGet("{orderId}/bids/admin")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<List<AdminBidResponseModel>>>> GetBidsForAdmin(string orderId)
    {
        var result = await _cargoOrderService.GetBidsForAdmin(orderId);
        return Ok(result);
    }

    // Dispatcher selects driver and creates bid
    [HttpPost("bids/dispatcher")]
    [Authorize(Roles = "dispatcher")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CreateBidOnBehalf([FromBody] CreateBidOnBehalfDto model)
    {
        var dispatcherId = User.FindFirst("DispatcherId")?.Value;
        model.DispatcherId = dispatcherId; // Ensure dispatcher can only bid for themselves

        var result = await _cargoOrderService.CreateBidOnBehalf(model);
        return Ok(result);
    }
}
```

### 9. Dispatcher Driver Onboarding Flow

#### 9.1 Complete Onboarding Process
```
1. Initial Driver Creation
   ├── Dispatcher provides basic driver info (name, email, phone, address, password)
   ├── System creates driver with inherited country
   ├── Commission structure established
   └── Driver status: OnboardingPending

2. Document Upload
   ├── Get required document types for dispatcher's country
   ├── Upload all required documents (ID, license, etc.)
   ├── Upload DOT/MC numbers (for US drivers)
   ├── Upload profile picture
   └── Driver status: OnboardingInReview

3. Truck Addition
   ├── Add truck details (using existing DriverAddTruckRequestModel)
   ├── Upload truck documents and pictures
   ├── Link truck to driver
   └── Driver status: OnboardingCompleted

4. Final Validation & Submission
   ├── Validate all required documents uploaded
   ├── Validate truck added and configured
   ├── Submit for admin approval
   └── Driver status: OnboardingInReview (pending admin approval)

5. Admin Approval
   ├── Admin reviews all documents and truck details
   ├── Admin approves or rejects driver
   └── Driver status: OnboardingCompleted (approved) / OnboardingPending (rejected)
```

#### 9.2 Key Differences from Transporter Model
| Aspect | Transporter (Nigerian) | Dispatcher (US) |
|--------|----------------------|-----------------|
| **Truck Assignment** | Adds trucks separately, then assigns to drivers | Truck is directly tied to specific driver during onboarding |
| **Document Process** | May have different document requirements | Must complete all US-specific document requirements |
| **Approval Flow** | Varies by market | Must go through admin approval for US compliance |
| **Country Inheritance** | Manual selection | Automatic inheritance from dispatcher |

### 10. Dispatcher Bidding Flow

#### 10.1 Dispatcher UI Flow
1. **View Available Orders**: Dispatcher sees CargoOrders available for bidding in their country
2. **Select Driver**: Dispatcher selects one of their **approved** managed drivers from a dropdown/list
3. **Enter Bid Amount**: Dispatcher enters the bid amount for the selected driver
4. **Submit Bid**: System creates bid that references the selected driver

#### 9.2 Bid Visibility Matrix
| User Type | What They See | Hidden Information |
|-----------|---------------|-------------------|
| **CargoOwner** | Driver name, truck details, bid amount, dispatcher indicator (`isFromDispatcher`) | Dispatcher identity and commission details |
| **Admin** | Driver name, truck details, bid amount, dispatcher name, commission details | Nothing hidden |
| **Driver** | Their own bids (if they bid directly) | Dispatcher commission details |
| **Dispatcher** | All their bids with driver assignments | N/A |

#### 9.3 Key Implementation Points
- **Bid Entity**: Always references the actual driver/truck, never the dispatcher directly
- **Role-Based Responses**: Different response models based on user role
- **Commission Tracking**: Stored in bid for admin visibility but hidden from cargo owner
- **Notes Field**: Can contain dispatcher information for admin, but filtered for cargo owner

### 10. Security & Validation

#### 9.1 Authorization Policies
```csharp
public class DispatcherAuthorizationHandler : AuthorizationHandler<DispatcherRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DispatcherRequirement requirement)
    {
        var user = context.User;
        if (user.IsInRole("dispatcher") || user.IsInRole("admin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

#### 9.2 Validation Rules
```csharp
public class CreateBidOnBehalfValidator : AbstractValidator<CreateBidOnBehalfDto>
{
    public CreateBidOnBehalfValidator()
    {
        RuleFor(x => x.DispatcherId).NotEmpty().WithMessage("Dispatcher ID is required");
        RuleFor(x => x.DriverId).NotEmpty().WithMessage("Driver ID is required");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Bid amount must be greater than 0");

        // Validate dispatcher has permission to bid for driver
        RuleFor(x => x).MustAsync(ValidateDispatcherDriverRelationship)
            .WithMessage("Dispatcher does not have permission to bid for this driver");
    }
}
```

### 10. Testing Strategy

#### 10.1 Unit Tests
- Service layer methods for dispatcher functionality
- Commission calculation logic
- Bid creation and validation
- Driver assignment workflows

#### 10.2 Integration Tests
- End-to-end dispatcher registration flow
- Bidding workflow from dispatcher perspective
- Financial calculation and payout workflows
- Driver approval process for dispatcher-managed drivers

#### 10.3 Performance Tests
- Bidding system performance with multiple dispatchers
- Database query optimization for dispatcher-driver relationships
- Commission calculation performance at scale

### 11. Implementation Timeline

#### Phase 1 (Weeks 1-2): Foundation
- Database schema updates
- Entity model changes
- Basic service layer modifications

#### Phase 2 (Weeks 3-4): Core Functionality
- Dispatcher registration and management
- Driver addition with commission structure
- Basic bidding functionality

#### Phase 3 (Weeks 5-6): Advanced Features
- Financial flow implementation
- Comprehensive validation and security
- API endpoint completion

#### Phase 4 (Weeks 7-8): Testing & Deployment
- Unit and integration testing
- Performance optimization
- Production deployment with data migration

### 12. Risks & Mitigation

#### 12.1 Data Integrity Risks
- **Risk**: Commission calculation errors
- **Mitigation**: Comprehensive unit tests and audit logging

#### 12.2 Performance Risks
- **Risk**: Complex queries for dispatcher-driver relationships
- **Mitigation**: Database indexing and query optimization

#### 12.3 Security Risks
- **Risk**: Unauthorized bidding on behalf of drivers
- **Mitigation**: Robust authorization policies and validation

### 13. Backward Compatibility

#### 13.1 Existing TruckOwner Functionality
- All existing TruckOwner functionality remains unchanged
- Transporter functionality is preserved
- Existing drivers are not affected

#### 13.2 API Compatibility
- All existing endpoints remain functional
- New endpoints follow existing naming conventions
- Response models are backward compatible

### 14. Success Metrics

#### 14.1 Technical Metrics
- Zero breaking changes to existing functionality
- <200ms response time for bid creation
- 99.9% uptime during migration

#### 14.2 Business Metrics
- Successful onboarding of US dispatchers
- Accurate commission calculations
- Seamless driver management workflow

## Conclusion

This implementation plan provides a comprehensive approach to extending the TruckOwner entity to support US market dispatchers while maintaining backward compatibility and ensuring system integrity. The phased approach allows for careful testing and validation at each stage, minimizing risks to the existing system.

The solution leverages the existing architecture patterns and maintains consistency with current design principles, ensuring a smooth integration that feels natural within the existing codebase.