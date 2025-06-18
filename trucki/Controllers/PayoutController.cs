using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin,Manager")] // Ensure only admin/manager access
    public class PayoutController : ControllerBase
    {
        private readonly IDriverPayoutService _payoutService;
        private readonly TruckiDBContext _dbContext;
        private readonly ILogger<PayoutController> _logger;

        public PayoutController(
            IDriverPayoutService payoutService,
            TruckiDBContext dbContext,
            ILogger<PayoutController> logger)
        {
            _payoutService = payoutService;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Manually trigger weekly payout processing for all eligible drivers
        /// </summary>
        [HttpPost("process-weekly")]
        public async Task<IActionResult> ProcessWeeklyPayouts([FromBody] ProcessPayoutRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseModel<object>.Fail("Invalid request data", 400));
            }

            var result = await _payoutService.ProcessWeeklyPayoutsAsync(request.AdminId);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Process payout for a specific driver
        /// </summary>
        [HttpPost("process-driver")]
        public async Task<IActionResult> ProcessDriverPayout([FromBody] ProcessPayoutRequestModel request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.DriverId))
            {
                return BadRequest(ApiResponseModel<object>.Fail("Driver ID is required", 400));
            }

            var result = await _payoutService.ProcessDriverPayoutAsync(
                request.DriverId, 
                request.AdminId, 
                request.ForceProcess);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                400 => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get earnings projections for all drivers
        /// </summary>
        [HttpGet("earnings-projections")]
        public async Task<IActionResult> GetAllEarningsProjections()
        {
            var result = await _payoutService.GetAllDriverEarningsProjectionsAsync();
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Flag an order to exclude it from payouts
        /// </summary>
        [HttpPost("orders/{orderId}/flag")]
        public async Task<IActionResult> FlagOrder(string orderId, [FromBody] FlagOrderRequestModel request)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest(ApiResponseModel<object>.Fail("Order ID is required", 400));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseModel<object>.Fail("Invalid request data", 400));
            }

            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(ApiResponseModel<object>.Fail("Order not found", 404));
                }

                // Check if already flagged
                if (order.IsFlagged)
                {
                    return BadRequest(ApiResponseModel<object>.Fail("Order is already flagged", 400));
                }

                // Flag the order
                order.IsFlagged = true;
                order.FlagReason = request.Reason;
                order.FlaggedAt = DateTime.UtcNow;
                order.FlaggedBy = request.AdminId;
                order.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} flagged by admin {AdminId} with reason: {Reason}", 
                    orderId, request.AdminId, request.Reason);

                return Ok(ApiResponseModel<object>.Success("Order flagged successfully", null, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging order {OrderId}", orderId);
                return StatusCode(500, ApiResponseModel<object>.Fail("Error flagging order", 500));
            }
        }

        /// <summary>
        /// Resolve/unflag an order to include it in payouts again
        /// </summary>
        [HttpPost("orders/{orderId}/resolve-flag")]
        public async Task<IActionResult> ResolveFlaggedOrder(string orderId, [FromBody] ResolveFlagRequestModel request)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest(ApiResponseModel<object>.Fail("Order ID is required", 400));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseModel<object>.Fail("Invalid request data", 400));
            }

            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(ApiResponseModel<object>.Fail("Order not found", 404));
                }

                if (!order.IsFlagged)
                {
                    return BadRequest(ApiResponseModel<object>.Fail("Order is not flagged", 400));
                }

                // Resolve the flag
                order.IsFlagged = false;
                order.FlagResolvedAt = DateTime.UtcNow;
                order.FlagResolvedBy = request.AdminId;
                order.FlagResolutionNotes = request.ResolutionNotes;
                order.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} flag resolved by admin {AdminId} with notes: {Notes}", 
                    orderId, request.AdminId, request.ResolutionNotes);

                return Ok(ApiResponseModel<object>.Success("Order flag resolved successfully", null, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving flag for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseModel<object>.Fail("Error resolving order flag", 500));
            }
        }

        /// <summary>
        /// Get all flagged orders
        /// </summary>
        [HttpGet("flagged-orders")]
        public async Task<IActionResult> GetFlaggedOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var flaggedOrders = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .Where(o => o.IsFlagged)
                    .OrderByDescending(o => o.FlaggedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new
                    {
                        OrderId = o.Id,
                        PickupLocation = o.PickupLocation,
                        DeliveryLocation = o.DeliveryLocation,
                        DriverName = o.AcceptedBid != null && o.AcceptedBid.Truck != null && o.AcceptedBid.Truck.Driver != null 
                            ? o.AcceptedBid.Truck.Driver.Name : "Unknown",
                        DriverId = o.AcceptedBid != null && o.AcceptedBid.Truck != null 
                            ? o.AcceptedBid.Truck.DriverId : null,
                        FlagReason = o.FlagReason,
                        FlaggedAt = o.FlaggedAt,
                        FlaggedBy = o.FlaggedBy,
                        DriverEarnings = o.DriverEarnings ?? 0,
                        Status = o.Status
                    })
                    .ToListAsync();

                var totalCount = await _dbContext.Set<CargoOrders>()
                    .CountAsync(o => o.IsFlagged);

                var response = new
                {
                    Orders = flaggedOrders,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(ApiResponseModel<object>.Success("Flagged orders retrieved successfully", response, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flagged orders");
                return StatusCode(500, ApiResponseModel<object>.Fail("Error retrieving flagged orders", 500));
            }
        }

        /// <summary>
        /// Get payout statistics and summary
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetPayoutStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var payouts = await _dbContext.Set<DriverPayout>()
                    .Include(p => p.Driver)
                    .Where(p => p.ProcessedDate >= start && p.ProcessedDate <= end)
                    .ToListAsync();

                var completedOrders = await _dbContext.Set<CargoOrders>()
                    .Where(o => o.Status == CargoOrderStatus.Completed && 
                               o.UpdatedAt >= start && o.UpdatedAt <= end)
                    .ToListAsync();

                var statistics = new
                {
                    DateRange = new { Start = start, End = end },
                    TotalPayouts = payouts.Count,
                    SuccessfulPayouts = payouts.Count(p => p.Status == PayoutStatus.Completed),
                    FailedPayouts = payouts.Count(p => p.Status == PayoutStatus.Failed),
                    TotalPayoutAmount = payouts.Where(p => p.Status == PayoutStatus.Completed).Sum(p => p.Amount),
                    TotalCompletedOrders = completedOrders.Count,
                    FlaggedOrders = completedOrders.Count(o => o.IsFlagged),
                    TotalDriverEarnings = completedOrders.Where(o => !o.IsFlagged).Sum(o => o.DriverEarnings ?? 0),
                    PendingEarnings = completedOrders.Where(o => !o.IsFlagged).Sum(o => o.DriverEarnings ?? 0) - 
                                     payouts.Where(p => p.Status == PayoutStatus.Completed).Sum(p => p.Amount),
                    TopDriversByEarnings = payouts
                        .Where(p => p.Status == PayoutStatus.Completed)
                        .GroupBy(p => new { p.DriverId, DriverName = p.Driver.Name })
                        .Select(g => new
                        {
                            DriverId = g.Key.DriverId,
                            DriverName = g.Key.DriverName,
                            TotalEarnings = g.Sum(p => p.Amount),
                            PayoutCount = g.Count()
                        })
                        .OrderByDescending(x => x.TotalEarnings)
                        .Take(10)
                        .ToList()
                };

                return Ok(ApiResponseModel<object>.Success("Payout statistics retrieved successfully", statistics, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout statistics");
                return StatusCode(500, ApiResponseModel<object>.Fail("Error retrieving statistics", 500));
            }
        }

        /// <summary>
        /// Recalculate driver earnings for a specific order
        /// </summary>
        [HttpPost("orders/{orderId}/recalculate-earnings")]
        public async Task<IActionResult> RecalculateOrderEarnings(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest(ApiResponseModel<object>.Fail("Order ID is required", 400));
            }

            var result = await _payoutService.RecalculateDriverEarningsAsync(orderId);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                _ => StatusCode(500, result)
            };
        }
    }
}