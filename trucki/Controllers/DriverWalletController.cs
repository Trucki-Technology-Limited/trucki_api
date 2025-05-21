using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverWalletController : ControllerBase
    {
        private readonly IDriverWalletService _driverWalletService;
        private readonly ILogger<DriverWalletController> _logger;

        public DriverWalletController(
            IDriverWalletService driverWalletService,
            ILogger<DriverWalletController> logger)
        {
            _driverWalletService = driverWalletService;
            _logger = logger;
        }

        [HttpGet("balance")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<DriverWalletBalanceResponseModel>>> GetWalletBalance(string driverId)
        {
            try
            {
                var result = await _driverWalletService.GetWalletBalanceAsync(driverId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver wallet balance");
                return StatusCode(500, ApiResponseModel<DriverWalletBalanceResponseModel>.Fail(
                    "An error occurred while retrieving wallet balance", 500));
            }
        }

        [HttpGet("transactions")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>>> GetTransactionHistory(
            string driverId,
            [FromQuery] GetDriverWalletTransactionsQueryDto query)
        {
            try
            {
                var result = await _driverWalletService.GetTransactionHistoryAsync(driverId, query);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver wallet transactions");
                return StatusCode(500, ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>.Fail(
                    "An error occurred while retrieving transaction history", 500));
            }
        }

        [HttpGet("withdrawals")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<DriverWithdrawalScheduleResponseModel>>> GetWithdrawalSchedule(string driverId)
        {
            try
            {
                var result = await _driverWalletService.GetWithdrawalScheduleAsync(driverId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver withdrawal schedule");
                return StatusCode(500, ApiResponseModel<DriverWithdrawalScheduleResponseModel>.Fail(
                    "An error occurred while retrieving withdrawal schedule", 500));
            }
        }

        [HttpPost("credit")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CreditDriverDelivery(CreditDriverWalletRequestDto request)
        {
            try
            {
                var result = await _driverWalletService.CreditDeliveryAmountAsync(
                    request.DriverId,
                    request.OrderId,
                    request.Amount,
                    request.Description);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crediting driver for delivery");
                return StatusCode(500, ApiResponseModel<bool>.Fail(
                    "An error occurred while crediting driver wallet", 500));
            }
        }

        [HttpGet("earnings")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<DriverEarningsSummaryResponseModel>>> GetEarningsSummary(
            string driverId,
            [FromQuery] DriverEarningsSummaryRequestDto request)
        {
            try
            {
                var result = await _driverWalletService.GetEarningsSummaryAsync(
                    driverId,
                    request.StartDate,
                    request.EndDate);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver earnings summary");
                return StatusCode(500, ApiResponseModel<DriverEarningsSummaryResponseModel>.Fail(
                    "An error occurred while retrieving earnings summary", 500));
            }
        }

        [HttpGet("pending-withdrawals")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>>> GetPendingWithdrawals()
        {
            try
            {
                var result = await _driverWalletService.GetAllPendingWithdrawalsAsync();
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending driver withdrawals");
                return StatusCode(500, ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>.Fail(
                    "An error occurred while retrieving pending withdrawals", 500));
            }
        }

        [HttpPost("process-withdrawals")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<DriverWithdrawalResultModel>>> ProcessWeeklyWithdrawals(
            [FromBody] ProcessDriverWithdrawalRequestDto request)
        {
            try
            {
                var result = await _driverWalletService.ProcessWeeklyWithdrawalsAsync(request.AdminId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weekly driver withdrawals");
                return StatusCode(500, ApiResponseModel<DriverWithdrawalResultModel>.Fail(
                    "An error occurred while processing withdrawals", 500));
            }
        }

        [HttpPost("update-projections")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UpdateWithdrawalProjections()
        {
            try
            {
                var result = await _driverWalletService.UpdateWithdrawalProjectionsAsync();
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver withdrawal projections");
                return StatusCode(500, ApiResponseModel<bool>.Fail(
                    "An error occurred while updating withdrawal projections", 500));
            }
        }
    }
}