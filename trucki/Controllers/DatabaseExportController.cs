using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using trucki.DatabaseContext;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataExportController : ControllerBase
    {
        private readonly TruckiDBContext _context;
        private readonly ILogger<DataExportController> _logger;

        public DataExportController(TruckiDBContext context, ILogger<DataExportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("export-all")]
        public async Task<IActionResult> ExportAllData()
        {
            try
            {
                _logger.LogInformation("Starting database export...");

                var exportData = new Dictionary<string, object>();

                // Export all entities using reflection to get all DbSets
                var dbSetProperties = _context.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsGenericType && 
                               p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.DbSet<>))
                    .ToList();

                foreach (var property in dbSetProperties)
                {
                    var entityType = property.PropertyType.GetGenericArguments()[0];
                    var dbSet = property.GetValue(_context);
                    
                    if (dbSet != null)
                    {
                        var method = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .Where(m => m.Name == "ToListAsync" && m.GetParameters().Length == 2)
                            .First()
                            .MakeGenericMethod(entityType);

                        var data = await (dynamic)method.Invoke(null, new[] { dbSet, CancellationToken.None });
                        exportData[property.Name] = data;
                        
                        _logger.LogInformation($"Exported {property.Name}: {((IEnumerable<object>)data).Count()} records");
                    }
                }

                // Add metadata
                exportData["ExportMetadata"] = new
                {
                    ExportDate = DateTime.UtcNow,
                    TotalTables = exportData.Count - 1, // -1 for metadata itself
                    Version = "1.0",
                    DatabaseType = "TruckiDB"
                };

                // Serialize to JSON with options to handle circular references
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var jsonString = JsonSerializer.Serialize(exportData, options);

                // Write to file in root directory
                var fileName = $"database_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                
                await System.IO.File.WriteAllTextAsync(filePath, jsonString);

                _logger.LogInformation($"Database export completed successfully. File saved: {fileName}");

                return Ok(new
                {
                    Success = true,
                    Message = "Database exported successfully",
                    FileName = fileName,
                    FilePath = filePath,
                    ExportedTables = exportData.Keys.Where(k => k != "ExportMetadata").ToList(),
                    TotalRecords = exportData.Where(kv => kv.Key != "ExportMetadata")
                        .Sum(kv => ((IEnumerable<object>)kv.Value).Count())
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database export");
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error occurred during export",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("export-selective")]
        public async Task<IActionResult> ExportSelectiveData([FromBody] List<string> tableNames)
        {
            try
            {
                _logger.LogInformation("Starting selective database export...");

                var exportData = new Dictionary<string, object>();

                var dbSetProperties = _context.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsGenericType && 
                               p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.DbSet<>) &&
                               tableNames.Contains(p.Name))
                    .ToList();

                foreach (var property in dbSetProperties)
                {
                    var entityType = property.PropertyType.GetGenericArguments()[0];
                    var dbSet = property.GetValue(_context);
                    
                    if (dbSet != null)
                    {
                        var method = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .Where(m => m.Name == "ToListAsync" && m.GetParameters().Length == 2)
                            .First()
                            .MakeGenericMethod(entityType);

                        var data = await (dynamic)method.Invoke(null, new[] { dbSet, CancellationToken.None });

                        exportData[property.Name] = data;
                        
                        _logger.LogInformation($"Exported {property.Name}: {((IEnumerable<object>)data).Count()} records");
                    }
                }

                // Add metadata
                exportData["ExportMetadata"] = new
                {
                    ExportDate = DateTime.UtcNow,
                    ExportType = "Selective",
                    RequestedTables = tableNames,
                    ExportedTables = exportData.Keys.Where(k => k != "ExportMetadata").ToList(),
                    Version = "1.0",
                    DatabaseType = "TruckiDB"
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var jsonString = JsonSerializer.Serialize(exportData, options);

                var fileName = $"selective_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                
                await System.IO.File.WriteAllTextAsync(filePath, jsonString);

                _logger.LogInformation($"Selective database export completed. File saved: {fileName}");

                return Ok(new
                {
                    Success = true,
                    Message = "Selective database export completed successfully",
                    FileName = fileName,
                    FilePath = filePath,
                    ExportedTables = exportData.Keys.Where(k => k != "ExportMetadata").ToList(),
                    TotalRecords = exportData.Where(kv => kv.Key != "ExportMetadata")
                        .Sum(kv => ((IEnumerable<object>)kv.Value).Count())
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during selective database export");
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error occurred during selective export",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("list-tables")]
        public IActionResult ListAvailableTables()
        {
            try
            {
                var dbSetProperties = _context.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsGenericType && 
                               p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.DbSet<>))
                    .Select(p => new
                    {
                        TableName = p.Name,
                        EntityType = p.PropertyType.GetGenericArguments()[0].Name
                    })
                    .ToList();

                return Ok(new
                {
                    Success = true,
                    AvailableTables = dbSetProperties,
                    TotalTables = dbSetProperties.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while listing tables");
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error occurred while listing tables",
                    Error = ex.Message
                });
            }
        }
    }
}