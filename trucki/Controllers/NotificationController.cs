using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<NotificationResponseModel>>>> GetNotifications(
            [FromQuery] GetNotificationsQueryDto query)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.GetNotificationsAsync(userId, query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetNotificationCount")]
        public async Task<ActionResult<ApiResponseModel<NotificationCountResponseModel>>> GetNotificationCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.GetNotificationCountAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("mark-as-read/{notificationId}")]
        public async Task<ActionResult<ApiResponseModel<bool>>> MarkAsRead(string notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.MarkAsReadAsync(userId, notificationId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("mark-multiple-as-read")]
        public async Task<ActionResult<ApiResponseModel<bool>>> MarkMultipleAsRead(
            [FromBody] MarkMultipleNotificationsAsReadDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.MarkMultipleAsReadAsync(userId, model.NotificationIds);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("mark-all-as-read")]
        public async Task<ActionResult<ApiResponseModel<bool>>> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{notificationId}")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteNotification(string notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.DeleteNotificationAsync(userId, notificationId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("delete-multiple")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteMultiple(
            [FromBody] DeleteMultipleNotificationsDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _notificationService.DeleteMultipleNotificationsAsync(userId, model.NotificationIds);
            return StatusCode(result.StatusCode, result);

        }
    }
}