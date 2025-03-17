using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class ChatService : IChatService
    {
        private readonly TruckiDBContext _dbContext;

        public ChatService(TruckiDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<ChatMessage> SaveMessageAsync(
            string orderId, 
            string senderId, 
            string recipientId, 
            string text, 
            string senderName = null,
            List<string> imageUrls = null)
        {
            // Verify the order exists
            var order = await _dbContext.Set<CargoOrders>()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found");
            }

            // Create a new message
            var message = new ChatMessage
            {
                OrderId = orderId,
                SenderId = senderId,
                SenderName = senderName,
                RecipientId = recipientId,
                Text = text,
                Timestamp = DateTime.UtcNow,
                IsRead = false,
                ImageUrls = imageUrls ?? new List<string>()
            };

            // Add to database
            _dbContext.Set<ChatMessage>().Add(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }

        public async Task<ApiResponseModel<List<ChatMessage>>> GetChatHistoryAsync(string orderId)
        {
            try
            {
                // Verify the order exists
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<List<ChatMessage>>.Fail(
                        $"Order with ID {orderId} not found", 
                        404);
                }

                // Get the chat history for this order
                var messages = await _dbContext.Set<ChatMessage>()
                    .Where(m => m.OrderId == orderId)
                    .OrderByDescending(m => m.Timestamp)
                    .ToListAsync();

                return ApiResponseModel<List<ChatMessage>>.Success(
                    "Chat history retrieved successfully", 
                    messages, 
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<List<ChatMessage>>.Fail(
                    $"Error retrieving chat history: {ex.Message}", 
                    500);
            }
        }

        public async Task<ChatMessage> MarkMessageAsReadAsync(string messageId)
        {
            var message = await _dbContext.Set<ChatMessage>()
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                throw new Exception($"Message with ID {messageId} not found");
            }

            // Update message status
            message.IsRead = true;
            await _dbContext.SaveChangesAsync();

            return message;
        }

        public async Task<ApiResponseModel<int>> GetUnreadMessageCountAsync(string userId)
        {
            try
            {
                // Count unread messages for the user
                int count = await _dbContext.Set<ChatMessage>()
                    .CountAsync(m => m.RecipientId == userId && !m.IsRead);

                return ApiResponseModel<int>.Success(
                    "Unread message count retrieved", 
                    count, 
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<int>.Fail(
                    $"Error retrieving unread message count: {ex.Message}", 
                    500);
            }
        }
    }
}