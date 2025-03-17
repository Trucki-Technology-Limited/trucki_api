using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Get chat history for an order
        /// </summary>
        [HttpGet("GetChatHistory/{orderId}")]
        public async Task<IActionResult> GetChatHistory(string orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _chatService.GetChatHistoryAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get unread message count for the current user
        /// </summary>
        [HttpGet("GetUnreadMessageCount")]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _chatService.GetUnreadMessageCountAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}