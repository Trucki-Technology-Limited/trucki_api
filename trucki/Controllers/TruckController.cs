using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TruckController : ControllerBase
    {
        private readonly ITruckService _truckService;

        public TruckController(ITruckService truckService)
        {
            _truckService = truckService;
        }

        #region Existing Endpoints (Backward Compatibility)

        [HttpPost("AddNewTruck")]
        [Authorize(Roles = "admin,transporter")]
        public async Task<ActionResult<ApiResponseModel<string>>> AddNewTruck([FromBody] AddTruckRequestModel model)
        {
            var response = await _truckService.AddNewTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddDriverOwnedTruck")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<string>>> AddDriverOwnedTruck([FromBody] DriverAddTruckRequestModel model)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _truckService.AddDriverOwnedTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditTruck")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditTruck([FromBody] EditTruckRequestModel model)
        {
            var response = await _truckService.EditTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteTruck")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<string>>> DeleteTruck(string truckId)
        {
            var response = await _truckService.DeleteTruck(truckId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTruckById")]
        [Authorize(Roles = "admin,transporter")]
        public async Task<ActionResult<ApiResponseModel<AllTruckResponseModel>>> GetTruckById(string truckId)
        {
            var response = await _truckService.GetTruckById(truckId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllTrucks")]
        [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetAllTrucks()
        {
            var response = await _truckService.GetAllTrucks();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("SearchTrucks")]
        [Authorize(Roles = "admin,manager,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckResponseModel>>>> SearchTruck(string? searchWords)
        {
            var response = await _truckService.SearchTruck(searchWords);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTruckDocuments")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<string>>>> GetTruckDocuments(string truckId)
        {
            var response = await _truckService.GetTruckDocuments(truckId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AssignDriverToTruck")]
        [Authorize(Roles = "admin,transporter")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
        {
            var response = await _truckService.AssignDriverToTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("UpdateTruckStatus")]
        [Authorize(Roles = "admin,manager,chiefmanager,transporter")]
        public async Task<ActionResult<ApiResponseModel<string>>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
        {
            var response = await _truckService.UpdateTruckStatus(truckId, model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTrucksByOwnersId")]
        [Authorize(Roles = "transporter")]
        public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetTrucksByOwnersId(string ownersId)
        {
            var response = await _truckService.GetTrucksByOwnersId(ownersId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTrucksByDriverId")]
        [Authorize(Roles = "driver,admin")]
        public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetTrucksByDriverId(string driverId)
        {
            var response = await _truckService.GetTrucksByDriverId(driverId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTruckStatusCountByOwnerId")]
        [Authorize(Roles = "transporter,dispatcher")]
        public async Task<ActionResult<ApiResponseModel<TruckStatusCountResponseModel>>> GetTruckStatusCountByOwnerId(string ownerId)
        {
            var response = await _truckService.GetTruckStatusCountByOwnerId(ownerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("UpdateApprovalStatus")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<string>>> UpdateApprovalStatus(string truckId, [FromBody] ApprovalStatus approvalStatus)
        {
            var response = await _truckService.UpdateApprovalStatusAsync(truckId, approvalStatus);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("UpdateTruckPhotos")]
        [Authorize(Roles = "admin,driver,transporter")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UpdateTruckPhotos([FromBody] UpdateTruckPhotosRequestModel model)
        {
            var response = await _truckService.UpdateTruckPhotos(model);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

        #region Enhanced Endpoints

        /// <summary>
        /// Get all trucks with enhanced filtering, pagination, and search capabilities
        /// Supports: pagination, search by plate number/name, filtering by type/status/approval, sorting
        /// </summary>
        [HttpGet("GetAllTrucksEnhanced")]
        [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>>> GetAllTrucksEnhanced(
            [FromQuery] GetTrucksQueryDto query)
        {
            try
            {
                var response = await _truckService.GetAllTrucksEnhancedAsync(query);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get comprehensive truck status and approval status counts for dashboard
        /// Returns counts for all status combinations and derived metrics
        /// </summary>
        [HttpGet("GetTruckStatusCounts")]
        [Authorize(Roles = "admin,manager,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<EnhancedTruckStatusCountResponseModel>>> GetTruckStatusCounts()
        {
            try
            {
                var response = await _truckService.GetTruckStatusCountsEnhancedAsync();
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get detailed truck information by ID including driver details and trip statistics
        /// Returns complete truck info with current trip status and order history
        /// </summary>
        [HttpGet("GetTruckDetailsById")]
        [Authorize(Roles = "admin,transporter,manager,dispatcher")]
        public async Task<ActionResult<ApiResponseModel<TruckDetailResponseModel>>> GetTruckDetailsById(
            [FromQuery] string truckId)
        {
            try
            {
                if (string.IsNullOrEmpty(truckId))
                {
                    return BadRequest(new ApiResponseModel<TruckDetailResponseModel>
                    {
                        IsSuccessful = false,
                        Message = "Truck ID is required",
                        StatusCode = 400,
                        Data = null
                    });
                }

                var response = await _truckService.GetTruckByIdEnhancedAsync(truckId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<TruckDetailResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Export trucks data as CSV file with filtering support
        /// Downloads CSV file with all truck information based on applied filters
        /// </summary>
        [HttpGet("ExportTrucksAsCsv")]
        [Authorize(Roles = "admin,manager,chiefmanager")]
        public async Task<IActionResult> ExportTrucksAsCsv([FromQuery] GetTrucksQueryDto query)
        {
            try
            {
                var result = await _truckService.ExportTrucksAsCsvAsync(query ?? new GetTrucksQueryDto());
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Error exporting trucks: {ex.Message}",
                    success = false
                });
            }
        }

        /// <summary>
        /// Update truck approval status with enhanced validation and workflow
        /// Supports rejection reasons and automatic status updates
        /// </summary>
        [HttpPost("UpdateApprovalStatusEnhanced")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<string>>> UpdateApprovalStatusEnhanced(
            [FromQuery] string truckId,
            [FromBody] UpdateApprovalStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(truckId))
                {
                    return BadRequest(new ApiResponseModel<string>
                    {
                        IsSuccessful = false,
                        Message = "Truck ID is required",
                        StatusCode = 400,
                        Data = null
                    });
                }

                if (request == null)
                {
                    return BadRequest(new ApiResponseModel<string>
                    {
                        IsSuccessful = false,
                        Message = "Update request body is required",
                        StatusCode = 400,
                        Data = null
                    });
                }

                var response = await _truckService.UpdateApprovalStatusEnhancedAsync(truckId, request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get truck status counts for a specific truck owner
        /// Returns detailed statistics for truck owner dashboard
        /// </summary>
        [HttpGet("GetTruckStatusCountsByOwnerId")]
        [Authorize(Roles = "transporter,admin,dispatcher")]
        public async Task<ActionResult<ApiResponseModel<EnhancedTruckStatusCountResponseModel>>> GetTruckStatusCountsByOwnerId(
            [FromQuery] string ownerId)
        {
            try
            {
                // For truck owners, ensure they can only access their own data
                if (User.IsInRole("transporter,dispatcher"))
                {
                    var userOwnerId = HttpContext.User.FindFirst("OwnerId")?.Value; // Adjust claim name as needed
                    if (!string.IsNullOrEmpty(userOwnerId) && userOwnerId != ownerId)
                    {
                        return Forbid("You can only access your own truck statistics");
                    }
                }

                if (string.IsNullOrEmpty(ownerId))
                {
                    return BadRequest(new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                    {
                        IsSuccessful = false,
                        Message = "Owner ID is required",
                        StatusCode = 400,
                        Data = null
                    });
                }

                var response = await _truckService.GetTruckStatusCountsByOwnerIdAsync(ownerId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get trucks by owner with enhanced filtering and pagination
        /// Enhanced version of GetTrucksByOwnersId with pagination support
        /// </summary>
        [HttpGet("GetTrucksByOwnerEnhanced")]
        [Authorize(Roles = "transporter,admin")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>>> GetTrucksByOwnerEnhanced(
            [FromQuery] string ownerId,
            [FromQuery] GetTrucksQueryDto query)
        {
            try
            {
                // For truck owners, ensure they can only access their own trucks
                if (User.IsInRole("transporter"))
                {
                    var userOwnerId = HttpContext.User.FindFirst("OwnerId")?.Value; // Adjust claim name as needed
                    if (!string.IsNullOrEmpty(userOwnerId) && userOwnerId != ownerId)
                    {
                        return Forbid("You can only access your own trucks");
                    }
                }

                if (string.IsNullOrEmpty(ownerId))
                {
                    return BadRequest(new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                    {
                        IsSuccessful = false,
                        Message = "Owner ID is required",
                        StatusCode = 400,
                        Data = null
                    });
                }

                // Set the owner filter in the query
                if (query == null) query = new GetTrucksQueryDto();
                query.TruckOwnerId = ownerId;

                var response = await _truckService.GetAllTrucksEnhancedAsync(query);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get trucks by driver with enhanced filtering (for driver-owned trucks)
        /// Enhanced version for drivers to view their truck with full statistics
        /// </summary>
        [HttpGet("GetTrucksByDriverEnhanced")]
        [Authorize(Roles = "driver,admin")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>>> GetTrucksByDriverEnhanced(
            [FromQuery] string? driverId,
            [FromQuery] GetTrucksQueryDto query)
        {
            try
            {
                // For drivers, use their own ID if not provided or if they're not admin
                if (User.IsInRole("driver"))
                {
                    var userDriverId = HttpContext.User.FindFirst("DriverId")?.Value; // Adjust claim name as needed
                    if (!string.IsNullOrEmpty(userDriverId))
                    {
                        driverId = userDriverId;
                    }
                }

                if (string.IsNullOrEmpty(driverId))
                {
                    return BadRequest(new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                    {
                        IsSuccessful = false,
                        Message = "Driver ID is required",
                        StatusCode = 400,
                        Data = null
                    });
                }

                // Create a custom query to filter by driver-owned trucks
                if (query == null) query = new GetTrucksQueryDto();
                
                // Use enhanced search with custom filtering
                var allTrucksResponse = await _truckService.GetAllTrucksEnhancedAsync(query);
                
                if (allTrucksResponse.IsSuccessful && allTrucksResponse.Data?.Data != null)
                {
                    // Filter for this driver's trucks
                    var driverTrucks = allTrucksResponse.Data.Data
                        .Where(t => t.DriverId == driverId && t.IsDriverOwnedTruck)
                        .ToList();

                    var pagedResponse = new PagedResponse<EnhancedTruckResponseModel>
                    {
                        Data = driverTrucks,
                        PageNumber = query.PageNumber,
                        PageSize = query.PageSize,
                        TotalCount = driverTrucks.Count,
                        TotalPages = (int)Math.Ceiling(driverTrucks.Count / (double)query.PageSize)
                    };

                    return Ok(new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                    {
                        IsSuccessful = true,
                        Message = $"Found {driverTrucks.Count} trucks for driver",
                        StatusCode = 200,
                        Data = pagedResponse
                    });
                }

                return StatusCode(allTrucksResponse.StatusCode, allTrucksResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                {
                    IsSuccessful = false,
                    Message = $"Internal server error: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        #endregion
    }
}