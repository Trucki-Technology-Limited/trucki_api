using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using trucki.DatabaseContext;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json.Serialization;
using trucki.Entities;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataImportController : ControllerBase
    {
        private readonly TruckiDBContext _context;
        private readonly ILogger<DataImportController> _logger;

        public DataImportController(TruckiDBContext context, ILogger<DataImportController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpPost("import-table")]
public async Task<IActionResult> ImportTableData([FromBody] ImportRequest request)
{
    try
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), request.FileName);

        if (!System.IO.File.Exists(filePath))
        {
            return BadRequest(new
            {
                Success = false,
                Message = $"File '{request.FileName}' not found in root directory"
            });
        }

        if (string.IsNullOrWhiteSpace(request.TableName))
        {
            return BadRequest(new
            {
                Success = false,
                Message = "TableName is required"
            });
        }

        _logger.LogInformation($"Starting import of table {request.TableName} from file: {request.FileName}");

        var importResults = new Dictionary<string, ImportResult>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var document = await JsonDocument.ParseAsync(fileStream);

            if (document.RootElement.TryGetProperty(request.TableName, out var tableData))
            {
                var result = await ImportTable(request.TableName, tableData, document);
                importResults[request.TableName] = result;

                if (result.Added > 0)
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Imported {request.TableName}: {result.Added} new, {result.Skipped} skipped");

                await transaction.CommitAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Table {request.TableName} imported successfully",
                    ImportResults = importResults,
                    TotalRecordsAdded = result.Added,
                    TotalRecordsSkipped = result.Skipped
                });
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Table '{request.TableName}' not found in file"
                });
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error occurred while importing table {request.TableName}");
            return BadRequest(new
            {
                Success = false,
                Message = $"Error occurred while importing table {request.TableName}",
                Error = ex.Message
            });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred during import");
        return BadRequest(new
        {
            Success = false,
            Message = "Error occurred during import",
            Error = ex.Message
        });
    }
}


        [HttpPost("import-all")]
        public async Task<IActionResult> ImportAllData([FromBody] ImportRequest request)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), request.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"File '{request.FileName}' not found in root directory"
                    });
                }

                _logger.LogInformation($"Starting import from file: {request.FileName}");

                var importResults = new Dictionary<string, ImportResult>();

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    await ClearIdentityTables();
                    
                    // Read and parse JSON file
                    await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    using var document = await JsonDocument.ParseAsync(fileStream);

                    // Get import order (dependency-aware)
                    var importOrder = GetImportOrder();
                    
                    foreach (var tableName in importOrder)
                    {
                        if (document.RootElement.TryGetProperty(tableName, out var tableData) && tableName != "ExportMetadata")
                        {
                            try
                            {
                                var result = await ImportTable(tableName, tableData, document);
                                importResults[tableName] = result;
                                
                                // Save changes for this table before moving to next
                                if (result.Added > 0)
                                {
                                    await _context.SaveChangesAsync();
                                }
                                _logger.LogInformation($"Imported {tableName}: {result.Added} new, {result.Skipped} skipped");
                                
                                // Clear change tracker to prevent tracking conflicts
                                _context.ChangeTracker.Clear();
                            }
                            catch (Exception ex)
                            {
                                var error = $"Error importing {tableName}: {ex.Message}";
                                _logger.LogError(ex, error);
                                throw;
                            }
                        }
                    }

                    // Handle circular dependency: Update CargoOrders with AcceptedBidId after Bids are imported
                    await UpdateCargoOrdersWithAcceptedBids(document);
                    
                    await transaction.CommitAsync();

                    _logger.LogInformation("Database import completed successfully");

                    return Ok(new
                    {
                        Success = true,
                        Message = "Database import completed successfully",
                        ImportResults = importResults,
                        TotalRecordsAdded = importResults.Values.Sum(r => r.Added),
                        TotalRecordsSkipped = importResults.Values.Sum(r => r.Skipped)
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database import");
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error occurred during import",
                    Error = ex.Message
                });
            }
        }

       private async Task UpdateCargoOrdersWithAcceptedBids(JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("CargoOrders", out var cargoOrdersData))
                return;

            _logger.LogInformation("Updating CargoOrders with AcceptedBidId references...");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            foreach (var recordElement in cargoOrdersData.EnumerateArray())
            {
                if (recordElement.TryGetProperty("Id", out var idElement) && 
                    recordElement.TryGetProperty("AcceptedBidId", out var acceptedBidIdElement))
                {
                    var cargoOrderId = idElement.GetString();
                    var acceptedBidId = acceptedBidIdElement.GetString();

                    if (!string.IsNullOrEmpty(cargoOrderId) && !string.IsNullOrEmpty(acceptedBidId))
                    {
                        try
                        {
                            // Check if the bid exists using the same method as EntityExistsInDatabase
                            var bidExists = await EntityExistsInDatabase("Bid", acceptedBidId);

                            if (bidExists)
                            {
                                // Update the CargoOrder with the AcceptedBidId
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE \"CargoOrders\" SET \"AcceptedBidId\" = {0} WHERE \"Id\" = {1}",
                                    acceptedBidId, cargoOrderId);
                                    
                                _logger.LogDebug($"Updated CargoOrder {cargoOrderId} with AcceptedBidId {acceptedBidId}");
                            }
                            else
                            {
                                _logger.LogWarning($"Bid with Id {acceptedBidId} not found for CargoOrder {cargoOrderId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error updating CargoOrder {cargoOrderId} with AcceptedBidId {acceptedBidId}");
                        }
                    }
                }
            }

            _logger.LogInformation("Completed updating CargoOrders with AcceptedBidId references");
        }

// Replace your entire ImportTable method with this updated version:

private async Task<ImportResult> ImportTable(string tableName, JsonElement tableData, JsonDocument fullDocument)
{
    var dbSetProperty = _context.GetType().GetProperty(tableName);
    if (dbSetProperty == null)
    {
        throw new InvalidOperationException($"Table {tableName} not found in context");
    }

    var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
    var dbSet = dbSetProperty.GetValue(_context);

    if (dbSet == null)
    {
        throw new InvalidOperationException($"DbSet for {tableName} is null");
    }

    var result = new ImportResult();

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Process each record in the array
    foreach (var recordElement in tableData.EnumerateArray())
    {
        if (tableName == "Routes" || tableName == "RoutesEnumerable")
        {
            // First, deserialize the route
            var entity = JsonSerializer.Deserialize<Routes>(recordElement.GetRawText(), options);
            if (entity == null) continue;

            // Clear the Business navigation property to prevent conflicts
            entity.Business = null;

            // Handle Business mapping if Business data exists in the JSON
            if (recordElement.TryGetProperty("Business", out var businessElement))
            {
                Business? existingBusiness = null;

                // Try to find existing business by Id first
                if (businessElement.TryGetProperty("Id", out var businessIdElement))
                {
                    var businessId = businessIdElement.GetString();
                    if (!string.IsNullOrWhiteSpace(businessId))
                    {
                        existingBusiness = await _context.Businesses
                            .FirstOrDefaultAsync(b => b.Id == businessId);
                    }
                }

                // If not found by Id, try by Name
                if (existingBusiness == null && businessElement.TryGetProperty("Name", out var businessNameElement))
                {
                    var businessName = businessNameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(businessName))
                    {
                        existingBusiness = await _context.Businesses
                            .FirstOrDefaultAsync(b => b.Name == businessName);
                    }
                }

                if (existingBusiness != null)
                {
                    // Set the BusinessId foreign key
                    entity.BusinessId = existingBusiness.Id;
                    entity.Business = existingBusiness;
                    _logger.LogDebug($"Attached existing business {existingBusiness.Name} to route {entity.Name}");
                }
                else
                {
                    // Create new business if it doesn't exist
                    var newBusiness = JsonSerializer.Deserialize<Business>(businessElement.GetRawText(), options);
                    if (newBusiness != null)
                    {
                        // Clear circular references
                        newBusiness.Routes = null;
                        newBusiness.Customers = null;
                        
                        _context.Businesses.Add(newBusiness);
                        await _context.SaveChangesAsync(); // Save to get the Id
                        
                        entity.BusinessId = newBusiness.Id;
                        entity.Business = newBusiness;
                        _logger.LogDebug($"Created new business {newBusiness.Name} for route {entity.Name}");
                    }
                }
            }

            // Now handle the Route itself
            var existingRoute = await _context.RoutesEnumerable.FindAsync(entity.Id);
            if (existingRoute != null)
            {
                // Update specific properties you want to change
                existingRoute.Name = entity.Name;
                existingRoute.FromRoute = entity.FromRoute;
                existingRoute.ToRoute = entity.ToRoute;
                existingRoute.Price = entity.Price;
                existingRoute.Gtv = entity.Gtv;
                existingRoute.IsActive = entity.IsActive;
                existingRoute.Ntons = entity.Ntons;
                existingRoute.FromRouteLat = entity.FromRouteLat;
                existingRoute.FromRouteLng = entity.FromRouteLng;
                existingRoute.ToRouteLat = entity.ToRouteLat;
                existingRoute.ToRouteLng = entity.ToRouteLng;
                existingRoute.BusinessId = entity.BusinessId;
                existingRoute.Business = entity.Business;
                existingRoute.UpdatedAt = DateTime.UtcNow; // Update timestamp
                
                // Save immediately after each update
                await _context.SaveChangesAsync();
                result.Updated++;
                _logger.LogDebug($"Updated and saved route {entity.Name} with BusinessId: {entity.BusinessId}");
            }
            else
            {
                // Add new route
                _context.RoutesEnumerable.Add(entity);
                result.Added++;
                _logger.LogDebug($"Added new route {entity.Name}");
            }
        }
        else
        {
            // Handle other entity types
            var entity = JsonSerializer.Deserialize(recordElement.GetRawText(), entityType, options);
            
            if (entity != null)
            {
                var idProperty = entityType.GetProperty("Id");
                var idValue = idProperty?.GetValue(entity);
                
                if (idValue != null)
                {
                    var existsInDb = await EntityExistsInDatabase(tableName, idValue);
                    if (!existsInDb)
                    {
                        ClearAllNavigationProperties(entity, entityType);
                        
                        var addMethod = dbSet.GetType().GetMethod("Add");
                        addMethod?.Invoke(dbSet, new[] { entity });
                        result.Added++;
                    }
                    else
                    {
                        result.Skipped++;
                    }
                }
            }
        }
    }

    return result;
}
        private async Task<bool> EntityExistsInDatabase(string tableName, object idValue)
        {
            try
            {
                var dbSetProperty = _context.GetType().GetProperty(tableName);
                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                var dbSet = (IQueryable<object>)dbSetProperty.GetValue(_context);

                return await dbSet.AnyAsync(e => EF.Property<object>(e, "Id").Equals(idValue));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error checking entity existence for {tableName}: {ex.Message}");
                // Fallback: Use Any() which doesn't load the full entity
                var dbSetProperty = _context.GetType().GetProperty(tableName);
                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                var dbSet = (IQueryable<object>)dbSetProperty.GetValue(_context);
                
                // Use Any() instead of Find() to avoid loading entities
                return await dbSet.AnyAsync(e => EF.Property<object>(e, "Id").Equals(idValue));
            }
        }

        private void ClearAllNavigationProperties(object entity, Type entityType)
        {
            // Get all properties that could be navigation properties
            var allProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            foreach (var property in allProperties)
            {
                if (IsNavigationProperty(property))
                {
                    try
                    {
                        // Set navigation property to null
                        property.SetValue(entity, null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"Could not clear navigation property {property.Name}: {ex.Message}");
                    }
                }
            }
        }

        public class CountResult
        {
            public int Value { get; set; }
        }

        private bool IsNavigationProperty(PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            // Skip properties that are definitely not navigation properties
            if (propertyType.IsValueType || propertyType == typeof(string) || propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                return false;

            // Check if property has ForeignKey attribute (this is the FK property, not navigation)
            var foreignKeyAttr = property.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKeyAttr != null) return false;

            // Check if it's an entity type (has an Id property and is a class)
            if (propertyType.IsClass && 
                propertyType != typeof(string) && 
                !propertyType.IsGenericType &&
                propertyType.GetProperty("Id") != null)
            {
                return true;
            }

            // Check if it's a collection navigation property
            if (propertyType.IsGenericType)
            {
                var genericTypeDef = propertyType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(ICollection<>) || 
                    genericTypeDef == typeof(List<>) || 
                    genericTypeDef == typeof(IList<>) ||
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType))
                {
                    return true;
                }
            }

            return false;
        }
        
        private async Task ClearIdentityTables()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUserTokens\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUserLogins\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUserClaims\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUserRoles\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetRoleClaims\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUsers\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetRoles\"");
        }

        private List<string> GetImportOrder()
        {
            // Define import order based on dependencies (least dependent first)
            // This order is critical to avoid foreign key constraint violations
            return new List<string>
            {
                // Identity tables first (no dependencies)
                "Users",
                "Roles", 
                "UserRoles",
                "UserClaims",
                "UserLogins",
                "UserTokens",
                "RoleClaims",
                
                // Base entities without dependencies
                "BankDetails",
                "DocumentTypes",
                "TermsAndConditions",
                
                // Entities with user dependencies only
                "Managers",
                "TruckOwners",
                "CargoOwners",
                "Officers",
                
                // Business entities (depends on Manager)
                "Businesses",
                "RoutesEnumerable", // Routes depend on Business
                "Customers", // Customers depend on Business
                
                // Truck related (depends on TruckOwner)
                "Trucks",
                
                // Driver related (depends on TruckOwner and potentially Truck)
                "Drivers",
                "DriverDocuments", // Depends on Driver and DocumentType
                "DriverBankAccounts", // Depends on Driver
                
                // Order related (depends on Business, Customer, Manager, Routes, Truck)
                "Orders",
                "CargoOrders", // Import without AcceptedBidId first
                "CargoOrderItems", // Depends on CargoOrders
                "Bids", // Import after CargoOrders
                // AcceptedBidId will be updated after both tables are imported
                
                // Financial entities (depends on various entities)
                "Transactions", // Depends on Business, Order, Truck
                "CargoOwnerWallets", // Depends on CargoOwner
                "WalletTransactions", // Depends on CargoOwnerWallet and potentially CargoOrders
                "DriverWallets", // Depends on Driver
                "DriverWalletTransactions", // Depends on DriverWallet
                "DriverWithdrawalSchedules", // Depends on Driver
                "DriverPayouts", // Depends on Driver
                "Invoices", // Depends on various entities
                
                // Communication and tracking (depends on users and orders)
                "ChatMessages", // Depends on CargoOrders
                "DeviceTokens", // Depends on User
                "Notifications", // Depends on User
                "DeliveryLocationUpdates", // Depends on CargoOrders
                
                // Records and ratings (depends on multiple entities)
                "TermsAcceptanceRecords", // Depends on Driver
                "OrderCancellations", // Depends on orders
                "DriverRatings" // Depends on CargoOrder, Driver, CargoOwner
            };
        }

        [HttpGet("list-files")]
        public IActionResult ListAvailableFiles()
        {
            try
            {
                var rootDirectory = Directory.GetCurrentDirectory();
                var jsonFiles = Directory.GetFiles(rootDirectory, "*.json")
                    .Select(f => new
                    {
                        FileName = Path.GetFileName(f),
                        FullPath = f,
                        Size = new FileInfo(f).Length,
                        CreatedDate = new FileInfo(f).CreationTime,
                        ModifiedDate = new FileInfo(f).LastWriteTime
                    })
                    .ToList();

                return Ok(new
                {
                    Success = true,
                    AvailableFiles = jsonFiles,
                    TotalFiles = jsonFiles.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while listing files");
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error occurred while listing files",
                    Error = ex.Message
                });
            }
        }
    }

    public class ImportRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string? TableName { get; set; } // Optional for single-table imports
    }

    public class ImportResult
    {
        public int Added { get; set; } = 0;
        public int Skipped { get; set; } = 0;
        public int Updated { get; set; } = 0;
    }
}